using System;
using Grynwald.ChangeLog.Integrations.GitLab;
using Xunit;

namespace Grynwald.ChangeLog.Test.Integrations.GitLab
{
    /// <summary>
    /// Tests for <see cref="GitLabReferenceTextElement"/>
    /// </summary>
    public class GitLabReferenceTextElementTest
    {
        public class Constructor
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
                var ex = Record.Exception(() => new GitLabReferenceTextElement(
                    text,
                    new("http://example.com"),
                    new("example.com", "namespace", "project"),
                    new(new("example.com", "namespace", "project"), GitLabReferenceType.Issue, 23)));

                // ASSERT
                var argumentException = Assert.IsType<ArgumentException>(ex);
                Assert.Equal("text", argumentException.ParamName);
            }

            [Fact]
            public void Uri_must_not_be_null()
            {
                // ARRANGE

                // ACT 
                var ex = Record.Exception(() => new GitLabReferenceTextElement(
                    "some-text",
                    null!,
                    new("example.com", "namespace", "project"),
                    new(new("example.com", "namespace", "project"), GitLabReferenceType.Issue, 23)));

                // ASSERT
                var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
                Assert.Equal("uri", argumentNullException.ParamName);
            }

            [Fact]
            public void Current_project_must_not_be_null()
            {
                // ARRANGE

                // ACT 
                var ex = Record.Exception(() => new GitLabReferenceTextElement(
                    "some-text",
                    new("http://example.com"),
                    null!,
                    new(new("example.com", "namespace", "project"), GitLabReferenceType.Issue, 23)));

                // ASSERT
                var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
                Assert.Equal("currentProject", argumentNullException.ParamName);
            }

            [Fact]
            public void Reference_must_not_be_null()
            {
                // ARRANGE

                // ACT 
                var ex = Record.Exception(() => new GitLabReferenceTextElement(
                    "some-text",
                    new("http://example.com"),
                    new("example.com", "namespace", "project"),
                    null!));

                // ASSERT
                var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
                Assert.Equal("reference", argumentNullException.ParamName);
            }
        }

        public class NormalizedText
        {
            [Theory]
            // Reference to a item in the same project
            [InlineData("user", "project", "user", "project", 23, "#23")]
            [InlineData("USER", "project", "user", "project", 23, "#23")]
            [InlineData("user", "PROJECT", "user", "project", 23, "#23")]
            [InlineData("user", "project", "USER", "project", 23, "#23")]
            [InlineData("user", "project", "user", "PROJECT", 23, "#23")]
            [InlineData("group/subgroup", "project", "group/subgroup", "project", 23, "#23")]
            [InlineData("GROUP/SUBGROUP", "project", "group/subgroup", "project", 23, "#23")]
            [InlineData("group/subgroup", "PROJECT", "group/subgroup", "project", 23, "#23")]
            [InlineData("group/subgroup", "project", "GROUP/SUBGROUP", "project", 23, "#23")]
            [InlineData("group/subgroup", "project", "group/subgroup", "PROJECT", 23, "#23")]
            // Reference to an item in a different project within the same namespace
            [InlineData("user", "project1", "user", "project2", 23, "project2#23")]
            [InlineData("USER", "project1", "user", "project2", 23, "project2#23")]
            [InlineData("user", "PROJECT1", "user", "project2", 23, "project2#23")]
            [InlineData("user", "project1", "USER", "project2", 23, "project2#23")]
            [InlineData("user", "project1", "user", "PROJECT2", 23, "project2#23")]
            [InlineData("group/subgroup", "project1", "group/subgroup", "project2", 23, "project2#23")]
            [InlineData("GROUP/SUBGROUP", "project1", "group/subgroup", "project2", 23, "project2#23")]
            [InlineData("group/subgroup", "PROJECT1", "group/subgroup", "project2", 23, "project2#23")]
            [InlineData("group/subgroup", "project1", "GROUP/SUBGROUP", "project2", 23, "project2#23")]
            [InlineData("group/subgroup", "project1", "group/subgroup", "PROJECT2", 23, "project2#23")]
            // Reference to an item in a different namespace
            [InlineData("user1", "project1", "user2", "project2", 23, "user2/project2#23")]
            [InlineData("USER1", "project1", "user2", "project2", 23, "user2/project2#23")]
            [InlineData("user1", "PROJECT1", "user2", "project2", 23, "user2/project2#23")]
            [InlineData("user1", "project1", "USER2", "project2", 23, "user2/project2#23")]
            [InlineData("USER1", "project1", "user2", "PROJECT2", 23, "user2/project2#23")]
            [InlineData("group1/subgroup", "project1", "group2/subgroup", "project2", 23, "group2/subgroup/project2#23")]
            [InlineData("GROUP1/SUBGROUP", "project1", "group2/subgroup", "project2", 23, "group2/subgroup/project2#23")]
            [InlineData("group1/subgroup", "PROJECT1", "group2/subgroup", "project2", 23, "group2/subgroup/project2#23")]
            [InlineData("group1/subgroup", "project1", "GROUP2/SUBGROUP", "project2", 23, "group2/subgroup/project2#23")]
            [InlineData("group1/subgroup", "project1", "group2/subgroup", "PROJECT2", 23, "group2/subgroup/project2#23")]
            public void Normalized_text_returns_expected_text(string currentOwner, string currentRepo, string referenceOwner, string referenceRepo, int referenceId, string expectedText)
            {
                // ARRANGE
                var currentProject = new GitLabProjectInfo("example.com", currentOwner, currentRepo);
                var reference = new GitLabReference(new GitLabProjectInfo("example.com", referenceOwner, referenceRepo), GitLabReferenceType.Issue, referenceId);

                // ACT
                var sut = new GitLabReferenceTextElement("some-text", new("https://example.com"), currentProject, reference);

                // ASSERT
                Assert.Equal(expectedText, sut.NormalizedText);
            }
        }
    }
}
