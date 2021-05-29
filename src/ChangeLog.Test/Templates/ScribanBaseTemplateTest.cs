using System.Collections.Generic;
using System.IO;
using System.Linq;
using Scriban.Syntax;
using Xunit;
using Zio;

namespace Grynwald.ChangeLog.Test.Templates
{
    public abstract class ScribanBaseTemplateTest : TemplateTest
    {
        protected abstract IFileSystem CreateTemplateFileSystem();


        [Fact]
        public void Include_statements_are_valid()
        {
            // ARRANGE / ACT
            var templateFileSystem = CreateTemplateFileSystem();

            // Get all "include" statements from all template files
            var includes = templateFileSystem.EnumerateFiles("/", "*.*", SearchOption.AllDirectories)
                .Select(path => new
                {
                    Path = path,
                    Parsed = Scriban.Template.Parse(templateFileSystem.ReadAllText(path))
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
                    templateFileSystem.FileExists(include.IncludePath),
                    $"Included file '{include.IncludePath}' in template '{include.SourcePath}' does not exist.")
            );

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
