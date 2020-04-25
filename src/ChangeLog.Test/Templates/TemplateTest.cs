using System.IO;
using ApprovalTests;
using ApprovalTests.Reporters;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Templates;
using Grynwald.ChangeLog.Test.Tasks;
using Grynwald.Utilities.IO;
using Xunit;

namespace Grynwald.ChangeLog.Test.Templates
{
    [UseReporter(typeof(DiffReporter))]
    public abstract class TemplateTest : TestBase
    {
        protected void Approve(ApplicationChangeLog changeLog, ChangeLogConfiguration? configuration = null)
        {
            var sut = GetTemplateInstance(configuration ?? ChangeLogConfigurationLoader.GetDefaultConfiguration());

            using (var temporaryDirectory = new TemporaryDirectory())
            {
                var outputPath = Path.Combine(temporaryDirectory, "changelog.md");
                sut.SaveChangeLog(changeLog, outputPath);

                Assert.True(File.Exists(outputPath));

                var output = File.ReadAllText(outputPath);

                var writer = new ApprovalTextWriter(output);
                Approvals.Verify(writer, new ApprovalNamer(), Approvals.GetReporter());
            }
        }

        protected abstract ITemplate GetTemplateInstance(ChangeLogConfiguration configuration);
    }
}
