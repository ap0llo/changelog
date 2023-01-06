using System;
using Grynwald.ChangeLog.Integrations.GitHub;
using Xunit;

namespace Grynwald.ChangeLog.Test.Integrations.GitHub
{
    /// <summary>
    /// Tests for <see cref="GitHubFileReferenceTextElement"/>
    /// </summary>
    public class GitHubFileReferenceTextElementTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        public void Text_must_not_be_null_or_whitespace(string text)
        {
            // ARRANGE

            // ACT 
            var ex = Record.Exception(() => new GitHubFileReferenceTextElement(
                text: text,
                uri: new("http://example.com"),
                relativePath: "relativePath"
            ));

            // ASSERT
            var argumentException = Assert.IsType<ArgumentException>(ex);
            Assert.Equal("text", argumentException.ParamName);
        }

        [Fact]
        public void Uri_must_not_be_null()
        {
            // ARRANGE

            // ACT 
            var ex = Record.Exception(() => new GitHubFileReferenceTextElement(
                text: "some text",
                uri: null!,
                relativePath: "relativePath"
            ));

            // ASSERT
            var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
            Assert.Equal("uri", argumentNullException.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        public void RelativePath_must_not_be_null_or_whitespace(string relativePath)
        {
            // ARRANGE

            // ACT 
            var ex = Record.Exception(() => new GitHubFileReferenceTextElement(
                text: "Some Text",
                uri: new("http://example.com"),
                relativePath: relativePath
            ));

            // ASSERT
            var argumentException = Assert.IsType<ArgumentException>(ex);
            Assert.Equal("relativePath", argumentException.ParamName);
        }
    }
}
