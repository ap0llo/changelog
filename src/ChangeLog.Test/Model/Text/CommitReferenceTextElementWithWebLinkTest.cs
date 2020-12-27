using System;
using System.Collections.Generic;
using System.Text;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Model.Text;
using Xunit;

namespace Grynwald.ChangeLog.Test.Model.Text
{
    /// <summary>
    /// Tests for <see cref="CommitReferenceTextElementWithWebLink"/>
    /// </summary>
    public class CommitReferenceTextElementWithWebLinkTest
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
            var ex = Record.Exception(() => new CommitReferenceTextElementWithWebLink(text, TestGitIds.Id1, new Uri("http://example.com")));

            // ASSERT
            var argumentException = Assert.IsType<ArgumentException>(ex);
            Assert.Equal("text", argumentException.ParamName);
        }

        [Fact]
        public void Commit_id_must_not_be_null()
        {
            // ARRANGE
            var id = default(GitId);

            // ACT 
            var ex = Record.Exception(() => new CommitReferenceTextElementWithWebLink("some text", id, new Uri("http://example.com")));

            // ASSERT
            Assert.NotNull(ex);
            var argumentException = Assert.IsType<ArgumentException>(ex);
            Assert.Equal("id", argumentException.ParamName);
        }

        [Fact]
        public void Uri_must_not_be_null()
        {
            // ARRANGE

            // ACT 
            var ex = Record.Exception(() => new CommitReferenceTextElementWithWebLink("some-tex", TestGitIds.Id1, null!));

            // ASSERT
            var argumentException = Assert.IsType<ArgumentNullException>(ex);
            Assert.Equal("uri", argumentException.ParamName);
        }
    }
}
