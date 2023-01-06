using System;
using System.Linq;
using System.Threading.Tasks;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Model.Text;
using Grynwald.ChangeLog.Pipeline;
using Grynwald.ChangeLog.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Grynwald.ChangeLog.Test.Tasks
{
    /// <summary>
    /// Tests for <see cref="ParseMarkdownWebLinksTask"/>
    /// </summary>
    public class ParseMarkdownWebLinksTaskTest
    {
        private readonly ILogger<ParseMarkdownWebLinksTask> m_Logger;


        public ParseMarkdownWebLinksTaskTest(ITestOutputHelper testOutputHelper)
        {
            m_Logger = new XunitLogger<ParseMarkdownWebLinksTask>(testOutputHelper);
        }


        [Theory]
        [InlineData("[Some Link](https://example.com)", "Some Link", "https://example.com")]
        [InlineData("[](https://example.com)", "https://example.com/", "https://example.com")]
        [InlineData(" [Some Link](http://example.com)", "Some Link", "http://example.com")]
        [InlineData("[Some Link](http://example.com)  ", "Some Link", "http://example.com")]
        public async Task Plain_text_footers_that_are_markdown_links_are_replaced_with_weblinks(string footerText, string expectedText, string expectedUrl)
        {
            // ARRANGE
            var testData = new TestDataFactory();
            var changeLog = new ApplicationChangeLog()
            {
                testData.GetSingleVersionChangeLog("1.2.3", entries: new[]
                {
                    testData.GetChangeLogEntry(footers: new[]
                    {
                        new ChangeLogEntryFooter(new("name"), new PlainTextElement(footerText))
                    })
                })
            };

            var sut = new ParseMarkdownWebLinksTask(m_Logger);

            // ACT 
            var result = await sut.RunAsync(changeLog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);
            Assert.Collection(
                changeLog.ChangeLogs.SelectMany(x => x.AllEntries).SelectMany(x => x.Footers),
                footer =>
                {
                    var weblinkTextElement = Assert.IsType<WebLinkTextElement>(footer.Value);
                    Assert.Equal(expectedText, weblinkTextElement.Text);
                    Assert.Equal(new Uri(expectedUrl), weblinkTextElement.Uri);
                }
            );
        }

        [Theory]
        [InlineData("[Some Link](not-a-url)")]
        [InlineData("[Some Link](./relative/url)")]
        [InlineData("[Some Link](ftp://example.com)")]
        public async Task Markdown_links_that_are_not_web_links_are_ignored(string link)
        {
            // ARRANGE
            var testData = new TestDataFactory();
            var originalFooterValue = new PlainTextElement(link);
            var changeLog = new ApplicationChangeLog()
            {
                testData.GetSingleVersionChangeLog("1.2.3", entries: new[]
                {
                    testData.GetChangeLogEntry(footers: new[]
                    {
                        new ChangeLogEntryFooter(new("name"), originalFooterValue)
                    })
                })
            };

            var sut = new ParseMarkdownWebLinksTask(m_Logger);

            // ACT 
            var result = await sut.RunAsync(changeLog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);
            Assert.Collection(
                changeLog.ChangeLogs.SelectMany(x => x.AllEntries).SelectMany(x => x.Footers),
                footer => Assert.Same(originalFooterValue, footer.Value));
        }

        [Theory]
        [InlineData("[Some Link](http://example.com)")]
        public async Task Footers_that_are_not_plain_text_elements_are_ignored(string link)
        {
            // ARRANGE
            var testData = new TestDataFactory();
            var originalFooterValue = new CommitReferenceTextElement(link, TestGitIds.Id1);
            var changeLog = new ApplicationChangeLog()
            {
                testData.GetSingleVersionChangeLog("1.2.3", entries: new[]
                {
                    testData.GetChangeLogEntry(footers: new[]
                    {
                        new ChangeLogEntryFooter(new("name"), originalFooterValue)
                    })
                })
            };

            var sut = new ParseMarkdownWebLinksTask(m_Logger);

            // ACT 
            var result = await sut.RunAsync(changeLog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);
            Assert.Collection(
                changeLog.ChangeLogs.SelectMany(x => x.AllEntries).SelectMany(x => x.Footers),
                footer => Assert.Same(originalFooterValue, footer.Value));
        }
    }
}
