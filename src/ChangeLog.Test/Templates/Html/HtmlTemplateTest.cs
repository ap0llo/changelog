using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Model.Text;
using Grynwald.ChangeLog.Templates;
using Grynwald.ChangeLog.Templates.Html;
using Xunit;

namespace Grynwald.ChangeLog.Test.Templates.Html
{
    public class HtmlTemplateTest : TemplateTest
    {
        protected override ITemplate GetTemplateInstance(ChangeLogConfiguration configuration) => new HtmlTemplate(configuration);


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
            configuration.Template.Html.NormalizeReferences = true;

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
            configuration.Template.Html.NormalizeReferences = false;

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
        public void Table_of_contents_is_included_when_there_are_at_least_two_version()
        {
            // ARRANGE
            var testData = new TestDataFactory();

            var changeLog = new ApplicationChangeLog()
            {
                testData.GetSingleVersionChangeLog("4.5.6", entries: new[]
                {
                    testData.GetChangeLogEntry(type: "feat", summary: "Some feature")
                }),
                testData.GetSingleVersionChangeLog("1.2.3", entries: new[]
                {
                    testData.GetChangeLogEntry(type: "feat", summary: "Some other feature")
                })
            };

            // ACT / ASSERT
            Approve(changeLog);
        }

        [Fact]
        public void Table_of_contents_is_not_included_when_there_is_only_a_single_version()
        {
            // ARRANGE
            var testData = new TestDataFactory();

            var changeLog = new ApplicationChangeLog()
            {
                testData.GetSingleVersionChangeLog("1.2.3", entries: new[]
                {
                    testData.GetChangeLogEntry(type: "feat", summary: "Some other feature")
                })
            };

            // ACT / ASSERT
            Approve(changeLog);
        }

    }
}
