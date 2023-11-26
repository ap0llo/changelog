using System;
using Grynwald.ChangeLog.Integrations.GitHub;
using Xunit;

namespace Grynwald.ChangeLog.Test.Integrations.GitHub
{
    /// <summary>
    /// Tests for <see cref="GitHubReferenceTextElement"/>
    /// </summary>
    public class GitHubReferenceTextElementTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        public void Text_must_not_be_null_or_whitespace(string? text)
        {
            // ARRANGE

            // ACT 
            var ex = Record.Exception(() => new GitHubReferenceTextElement(
                text!,
                new("http://example.com"),
                new("example.com", "owner", "repo"),
                new(new("example.com", "owner", "repo"), 23)));

            // ASSERT
            var argumentException = Assert.IsType<ArgumentException>(ex);
            Assert.Equal("text", argumentException.ParamName);
        }

        [Fact]
        public void Uri_must_not_be_null()
        {
            // ARRANGE

            // ACT 
            var ex = Record.Exception(() => new GitHubReferenceTextElement(
                "some-text",
                null!,
                new("example.com", "owner", "repo"),
                new(new("example.com", "owner", "repo"), 23)));

            // ASSERT
            var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
            Assert.Equal("uri", argumentNullException.ParamName);
        }

        [Fact]
        public void Current_project_must_not_be_null()
        {
            // ARRANGE

            // ACT 
            var ex = Record.Exception(() => new GitHubReferenceTextElement(
                "some-text",
                new("http://example.com"),
                null!,
                new(new("example.com", "owner", "repo"), 23)));

            // ASSERT
            var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
            Assert.Equal("currentProject", argumentNullException.ParamName);
        }

        [Fact]
        public void Reference_must_not_be_null()
        {
            // ARRANGE

            // ACT 
            var ex = Record.Exception(() => new GitHubReferenceTextElement(
                "some-text",
                new("http://example.com"),
                new("example.com", "owner", "repo"),
                null!));

            // ASSERT
            var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
            Assert.Equal("reference", argumentNullException.ParamName);
        }

        [Theory]
        // Reference to a PR or issue in the same repository
        [InlineData("owner", "repo", "owner", "repo", 23, "#23")]
        [InlineData("OWNER", "repo", "owner", "repo", 23, "#23")]
        [InlineData("owner", "repo", "owner", "REPO", 23, "#23")]
        // Reference to a different repo with the same owner (user or organization)
        [InlineData("owner", "repo1", "owner", "repo2", 23, "owner/repo2#23")]
        [InlineData("OWNER", "repo1", "owner", "repo2", 23, "owner/repo2#23")]
        [InlineData("owner", "REPO1", "owner", "repo2", 23, "owner/repo2#23")]
        // Reference to a different repo with a different owner
        [InlineData("owner1", "repo1", "owner2", "repo2", 23, "owner2/repo2#23")]
        [InlineData("OWNER1", "repo1", "owner2", "repo2", 23, "owner2/repo2#23")]
        [InlineData("owner1", "REPO1", "owner2", "repo2", 23, "owner2/repo2#23")]
        [InlineData("owner1", "repo", "owner2", "repo", 23, "owner2/repo#23")]
        [InlineData("OWNER1", "repo", "owner2", "repo", 23, "owner2/repo#23")]
        [InlineData("owner1", "REPO", "owner2", "repo", 23, "owner2/repo#23")]
        // References must always be lower-case
        [InlineData("owner", "repo1", "OWNER", "repo2", 23, "owner/repo2#23")]
        [InlineData("owner", "repo1", "owner", "REPO2", 23, "owner/repo2#23")]
        [InlineData("owner1", "repo1", "OWNER2", "repo2", 23, "owner2/repo2#23")]
        [InlineData("owner1", "repo1", "owner2", "REPO2", 23, "owner2/repo2#23")]
        public void Normalized_text_returns_expected_text(string currentOwner, string currentRepo, string referenceOwner, string referenceRepo, int referenceId, string expectedText)
        {
            // ARRANGE
            var currentProject = new GitHubProjectInfo("example.com", currentOwner, currentRepo);
            var reference = new GitHubReference(new GitHubProjectInfo("example.com", referenceOwner, referenceRepo), referenceId);

            // ACT
            var sut = new GitHubReferenceTextElement("some-text", new("https://example.com"), currentProject, reference);

            // ASSERT
            Assert.Equal(expectedText, sut.NormalizedText);
        }
    }
}
