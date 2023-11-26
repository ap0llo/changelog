using System;
using System.Collections.Generic;
using Grynwald.ChangeLog.Integrations.GitHub;
using Xunit;

namespace Grynwald.ChangeLog.Test.Integrations.GitHub
{
    /// <summary>
    /// Tests for <see cref="GitHubReference"/>
    /// </summary>
    public class GitHubReferenceTest : EqualityTest<GitHubReference, GitHubReferenceTest>, IEqualityTestDataProvider<GitHubReference>
    {
        public IEnumerable<(GitHubReference left, GitHubReference right)> GetEqualTestCases()
        {
            var project = new GitHubProjectInfo("example.com", "owner", "repo");

            yield return (new GitHubReference(project, 23), new GitHubReference(project, 23));
        }

        public IEnumerable<(GitHubReference left, GitHubReference right)> GetUnequalTestCases()
        {
            var project1 = new GitHubProjectInfo("example.com", "owner", "repo");
            var project2 = new GitHubProjectInfo("example.com", "some-other-owner", "repo");

            yield return (new GitHubReference(project1, 23), new GitHubReference(project1, 42));
            yield return (new GitHubReference(project1, 23), new GitHubReference(project2, 23));
        }

        public class Constructor
        {
            [Fact]
            public void Project_must_not_be_null()
            {
                // ARRANGE

                // ACT 
                var ex = Record.Exception(() => new GitHubReference(null!, 23));

                // ASSERT
                var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
                Assert.Equal("project", argumentNullException.ParamName);
            }

            [Theory]
            [InlineData(0)]
            [InlineData(-1)]
            [InlineData(-23)]
            public void Id_must_not_be_zeo_or_negative(int id)
            {
                // ARRANGE
                var project = new GitHubProjectInfo("example.com", "owner", "repo");

                // ACT 
                var ex = Record.Exception(() => new GitHubReference(project, id));

                // ASSERT
                var argumentOutOfRangeException = Assert.IsType<ArgumentOutOfRangeException>(ex);
                Assert.Equal("id", argumentOutOfRangeException.ParamName);
            }
        }


        public class ToString_
        {
            [Theory]
            [InlineData("owner", "repo", 23, "owner/repo#23")]
            [InlineData("OWNER", "repo", 24, "owner/repo#24")]
            [InlineData("owner", "REPO", 13, "owner/repo#13")]
            public void ToString_returns_expected_value(string owner, string repo, int id, string expected)
            {
                // ARRANGE
                var project = new GitHubProjectInfo("example.com", owner, repo);
                var sut = new GitHubReference(project, id);

                // ACT 
                var actual = sut.ToString();

                // ASSERT
                Assert.Equal(expected, actual);
            }

            [Theory]
            [InlineData("owner", "repo", 23, GitHubReferenceFormat.Full, "owner/repo#23")]
            [InlineData("OWNER", "repo", 24, GitHubReferenceFormat.Full, "owner/repo#24")]
            [InlineData("owner", "REPO", 13, GitHubReferenceFormat.Full, "owner/repo#13")]
            [InlineData("owner", "repo", 23, GitHubReferenceFormat.Minimal, "#23")]
            public void ToString_returns_expected_value_when_format_is_specified(string owner, string repo, int id, GitHubReferenceFormat format, string expected)
            {
                // ARRANGE
                var project = new GitHubProjectInfo("example.com", owner, repo);
                var sut = new GitHubReference(project, id);

                // ACT 
                var actual = sut.ToString(format);

                // ASSERT
                Assert.Equal(expected, actual);
            }
        }


        public class TryParse
        {
            [Theory]
            [InlineData("#23", "owner", "repo", "owner", "repo", 23)]
            [InlineData("GH-23", "owner", "repo", "owner", "repo", 23)]
            [InlineData("anotherOwner/anotherRepo#42", "owner", "repo", "anotherOwner", "anotherRepo", 42)]
            [InlineData("another-Owner/another-Repo#42", "owner", "repo", "another-Owner", "another-Repo", 42)]
            [InlineData("another.Owner/another.Repo#42", "owner", "repo", "another.Owner", "another.Repo", 42)]
            [InlineData("another_Owner/another_Repo#42", "owner", "repo", "another_Owner", "another_Repo", 42)]
            // Trailing and leading whitespace must be ignored
            [InlineData(" #23", "owner", "repo", "owner", "repo", 23)]
            [InlineData("#23 ", "owner", "repo", "owner", "repo", 23)]
            [InlineData(" GH-23", "owner", "repo", "owner", "repo", 23)]
            [InlineData("GH-23 ", "owner", "repo", "owner", "repo", 23)]
            [InlineData(" anotherOwner/anotherRepo#42", "owner", "repo", "anotherOwner", "anotherRepo", 42)]
            [InlineData("anotherOwner/anotherRepo#42 ", "owner", "repo", "anotherOwner", "anotherRepo", 42)]
            public void TryParse_succeeds_for_parsable_references(string input, string currentRepositoryOwner, string currentRepositoryName, string expecedRepositoryOwner, string expectedRepositoryName, int expectedId)
            {
                // ARRANGE

                // ACT 
                var success = GitHubReference.TryParse(input, new("example.com", currentRepositoryOwner, currentRepositoryName), out var parsed);

                // ASSERT
                Assert.True(success);
                Assert.NotNull(parsed);
                Assert.Equal("example.com", parsed!.Project.Host);
                Assert.Equal(expecedRepositoryOwner, parsed.Project.Owner);
                Assert.Equal(expectedRepositoryName, parsed.Project.Repository);
                Assert.Equal(expectedId, parsed.Id);
            }

            [Theory]
            [InlineData(null)]
            [InlineData("")]
            [InlineData(" ")]
            [InlineData("\t")]
            [InlineData("not-a-reference")]
            [InlineData("Not a/reference#0")]
            [InlineData("Not a/reference#xyz")]
            [InlineData("#xyz")]
            [InlineData("GH-xyz")]
            [InlineData("#1 2 3")]
            [InlineData("GH-1 2 3")]
            public void TryParse_fails_for_invalid_references(string? input)
            {
                // ARRANGE
                var currentProject = new GitHubProjectInfo("example.com", "owner", "repo");

                // ACT 
                var success = GitHubReference.TryParse(input!, currentProject, out var result);

                // ASSERT
                Assert.False(success);
                Assert.Null(result);
            }

            [Fact]
            public void TryParse_throws_ArgumentNullException_when_current_project_is_null()
            {
                // ARRANGE

                // ACT
                var ex = Record.Exception(() => GitHubReference.TryParse("#23", null!, out var result));

                // ASSERT
                var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
                Assert.Equal("currentProject", argumentNullException.ParamName);
            }
        }
    }
}
