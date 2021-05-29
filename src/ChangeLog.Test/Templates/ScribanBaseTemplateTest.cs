using System.Collections.Generic;
using System.IO;
using System.Linq;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Templates;
using Grynwald.Utilities.IO;
using Scriban.Syntax;
using Xunit;
using Zio;

namespace Grynwald.ChangeLog.Test.Templates
{
    /// <summary>
    /// Common test cases for templates that derive from <see cref="ScribanBaseTemplate"/>
    /// </summary>
    public abstract class ScribanBaseTemplateTest : TemplateTest
    {
        protected abstract void SetCustomDirectory(ChangeLogConfiguration configuration, string customDirectory);

        [Fact]
        public void Include_statements_are_valid()
        {
            // ARRANGE / ACT
            var template = (ScribanBaseTemplate)GetTemplateInstance(new ChangeLogConfiguration());

            // Get all "include" statements from all template files
            var includes = template.FileSystem.EnumerateFiles("/", "*.*", SearchOption.AllDirectories)
                .Select(path => new
                {
                    Path = path,
                    Parsed = Scriban.Template.Parse(template.FileSystem.ReadAllText(path))
                })
                .SelectMany(template =>
                    EnumerateScriptNodes(template.Parsed.Page)
                        .OfType<ScriptExpressionStatement>()
                        .Where(x => x.Expression is ScriptFunctionCall { Target: ScriptVariable { Name: "include" } })
                        .Select(x => x.Expression)
                        .Cast<ScriptFunctionCall>()
                        .Select(includeStatement => (ScriptLiteral)includeStatement.Arguments.First())
                        .Select(literal => (UPath)(string)literal.Value)
                        .Select(includePath => new
                        {
                            SourcePath = template.Path,
                            IncludePath = includePath
                        })
            )
            .ToHashSet();

            // ASSERT
            Assert.All(includes, include =>
                Assert.True(
                    include.IncludePath.IsAbsolute,
                    $"Include path '{include.IncludePath}' in template file '{include.SourcePath}' must be absolute.")
            );

            Assert.All(includes, include =>
                Assert.True(
                    template.FileSystem.FileExists(include.IncludePath),
                    $"Included file '{include.IncludePath}' in template '{include.SourcePath}' does not exist.")
            );

        }

        [Fact]
        public void Files_from_CustomDirectory_override_template_files()
        {
            // ARRANGE
            using var customDir = new TemporaryDirectory();

            var configuration = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            SetCustomDirectory(configuration, customDir);

            var template = (ScribanBaseTemplate)GetTemplateInstance(configuration);
            var files = template.FileSystem.EnumerateFiles("/", "*.*", SearchOption.AllDirectories);

            // ACT 
            foreach (var file in files)
            {
                var expected = "Custom Content";

                var overrideFile = Path.Combine(customDir, file.FullName.TrimStart('/'));
                Directory.CreateDirectory(Path.GetDirectoryName(overrideFile)!);
                File.WriteAllText(overrideFile, expected);

                var actual = template.FileSystem.ReadAllText(file);

                // ASSERT
                Assert.Equal(expected, actual);

                File.Delete(overrideFile);
            }

        }


        /// <summary>
        /// Recursively enumerates all Scriban script nodes starting at the specified node.
        /// </summary>
        private IEnumerable<ScriptNode> EnumerateScriptNodes(ScriptNode node)
        {
            if (node is null)
                yield break;

            yield return node;

            if (node.Children is null)
                yield break;

            foreach (var child in node.Children)
            {
                foreach (var descendants in EnumerateScriptNodes(child))
                {
                    yield return descendants;
                }
            }
        }
    }
}
