using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Templates;
using Grynwald.ChangeLog.Templates.GitLabRelease;
using Xunit;

namespace Grynwald.ChangeLog.Test.Templates.GitLabRelease
{
    public class GitLabReleaseTemplateTest : TemplateTest
    {
        protected override ITemplate GetTemplateInstance(ChangeLogConfiguration configuration) => new GitLabReleaseTemplate(configuration);

        [Fact]
        public void ChangeLog_is_converted_to_expected_Markdown_01()
        {
            // Empty changelog
            Approve(new ApplicationChangeLog());
        }
    }
}
