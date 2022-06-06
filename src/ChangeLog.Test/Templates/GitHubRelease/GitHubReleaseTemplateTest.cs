using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Model.Text;
using Grynwald.ChangeLog.Templates;
using Grynwald.ChangeLog.Templates.GitHubRelease;
using Xunit;

namespace Grynwald.ChangeLog.Test.Templates.GitHubRelease
{
    public class GitHubReleaseTemplateTest : ScribanBaseTemplateTest
    {
        protected override ITemplate GetTemplateInstance(ChangeLogConfiguration configuration) => new GitHubReleaseTemplate(configuration);

        protected override void SetCustomDirectory(ChangeLogConfiguration configuration, string customDirectory)
        {
            configuration.Template.GitHubRelease.CustomDirectory = customDirectory;
        }


        [Fact]
        public void Name_returns_expected_value()
        {
            // ARRANGE
            var sut = GetTemplateInstance(ChangeLogConfigurationLoader.GetDefaultConfiguration());

            // ACT 
            var actual = sut.Name;

            // ASSERT
            Assert.Equal(TemplateName.GitHubRelease, actual);
        }

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

        private class CustomTextElement : INormalizedTextElement
        {
            public string NormalizedText { get; set; } = "";

            public TextStyle NormalizedStyle { get; set; }

            public string Text { get; set; } = "";

            public TextStyle Style { get; set; }
        }

        [Fact]
        public void When_normalization_is_enabled_normalized_text_is_rendered()
        {
            // ARRANGE
            var testData = new TestDataFactory();
            var configuration = new ChangeLogConfiguration();
            configuration.Template.GitHubRelease.NormalizeReferences = true;

            var footerValue = new CustomTextElement()
            {
                Text = "Text",
                Style = TextStyle.Code,
                NormalizedText = "NormalizedText",
                NormalizedStyle = TextStyle.None
            };

            var changeLog = new ApplicationChangeLog()
            {
                testData.GetSingleVersionChangeLog("1.2.3", entries: new[]
                {
                    testData.GetChangeLogEntry(footers: new[]
                    {
                        new ChangeLogEntryFooter(
                            new("Name"),
                            footerValue
                        )
                    })
                })
            };

            // ACT / ASSERT
            Approve(changeLog, configuration);
        }

        [Fact]
        public void When_normalization_is_disabled_default_text_is_rendered()
        {
            // ARRANGE
            var testData = new TestDataFactory();
            var configuration = new ChangeLogConfiguration();
            configuration.Template.GitHubRelease.NormalizeReferences = false;

            var footerValue = new CustomTextElement()
            {
                Text = "Text",
                Style = TextStyle.Code,
                NormalizedText = "NormalizedText",
                NormalizedStyle = TextStyle.None
            };

            var changeLog = new ApplicationChangeLog()
            {
                testData.GetSingleVersionChangeLog("1.2.3", entries: new[]
                {
                    testData.GetChangeLogEntry(footers: new[]
                    {
                        new ChangeLogEntryFooter(
                            new("Name"),
                            footerValue
                        )
                    })
                })
            };

            // ACT / ASSERT
            Approve(changeLog, configuration);
        }
    }
}
