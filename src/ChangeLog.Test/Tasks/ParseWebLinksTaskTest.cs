using System;
using System.Linq;
using System.Threading.Tasks;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Model.Text;
using Grynwald.ChangeLog.Pipeline;
using Grynwald.ChangeLog.Tasks;
using Xunit;

namespace Grynwald.ChangeLog.Test.Tasks
{
    /// <summary>
    /// Tests for <see cref="ParseWebLinksTask"/>
    /// </summary>
    public class ParseWebLinksTaskTest
    {
        [Theory]
        [InlineData("http://example.com", "http://example.com")]
        [InlineData("https://example.com", "https://example.com")]
        [InlineData("https://example.com/some-page", "https://example.com/some-page")]
        [InlineData("https://github.com/user/repo", "https://github.com/user/repo")]
        [InlineData("HTTPS://github.com/user/repo", "HTTPS://github.com/user/repo")]
        [InlineData("HTTP://example.com", "HTTP://example.com")]
        [InlineData("htTP://example.com", "htTP://example.com")]
        [InlineData("  http://example.com", "http://example.com")]
        [InlineData("http://example.com  ", "http://example.com")]
        public async Task Plain_text_footer_values_that_are_web_urls_are_replaced(string footerText, string expectedUrl)
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

            var sut = new ParseWebLinksTask();

            // ACT 
            var result = await sut.RunAsync(changeLog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);
            var footer = Assert.Single(changeLog.ChangeLogs.SelectMany(x => x.AllEntries).SelectMany(x => x.Footers));
            var weblinkTextElement = Assert.IsType<WebLinkTextElement>(footer.Value);
            Assert.Equal(new Uri(expectedUrl), weblinkTextElement.Uri);
        }

        [Theory]
        [InlineData("not-a-url")]
        [InlineData("./relative/url")]
        [InlineData("ftp://example.com")]
        public async Task Footers_that_are_not_web_urls_are_ignored(string text)
        {
            // ARRANGE
            var testData = new TestDataFactory();
            var originalFooterValue = new PlainTextElement(text);
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

            var sut = new ParseWebLinksTask();

            // ACT 
            var result = await sut.RunAsync(changeLog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);
            var footer = Assert.Single(changeLog.ChangeLogs.SelectMany(x => x.AllEntries).SelectMany(x => x.Footers));
            Assert.Same(originalFooterValue, footer.Value);
        }

        [Theory]
        [InlineData("http://example.com")]
        [InlineData("https://example.com")]
        [InlineData("https://example.com/some-page")]
        [InlineData("https://github.com/user/repo")]
        public async Task Footers_that_are_not_plain_text_elements_are_ignored(string url)
        {
            // ARRANGE
            var testData = new TestDataFactory();
            var originalFooterValue = new CommitReferenceTextElement(url, TestGitIds.Id1);
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

            var sut = new ParseWebLinksTask();

            // ACT 
            var result = await sut.RunAsync(changeLog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);
            var footer = Assert.Single(changeLog.ChangeLogs.SelectMany(x => x.AllEntries).SelectMany(x => x.Footers));
            Assert.Same(originalFooterValue, footer.Value);
        }
    }
}
