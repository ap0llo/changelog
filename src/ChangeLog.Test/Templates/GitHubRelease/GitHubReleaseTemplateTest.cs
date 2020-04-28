using System;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Templates;
using Grynwald.ChangeLog.Templates.GitHubRelease;
using Xunit;

namespace Grynwald.ChangeLog.Test.Templates.GitHubRelease
{
    public class GitHubReleaseTemplateTest : TemplateTest
    {
        protected override ITemplate GetTemplateInstance(ChangeLogConfiguration configuration) => new GitHubReleaseTemplate(configuration);


        [Fact]
        public void SaveChangeLog_throws_TemplateExecutionException_when_changelog_contains_multiple_versions()
        {
            // ARRANGE
            var changelog = new ApplicationChangeLog()
            {
                GetSingleVersionChangeLog("1.2.3"),
                GetSingleVersionChangeLog("4.5.6")
            };

            var sut = GetTemplateInstance(new ChangeLogConfiguration());

            // ACT / ASSERT
            Assert.Throws<TemplateExecutionException>(() => sut.SaveChangeLog(changelog, "Irrelevant"));
        }

    }
}
