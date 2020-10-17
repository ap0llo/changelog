using System;
using System.Collections.Generic;
using Grynwald.ChangeLog.Git;
using Octokit;
using Xunit;

namespace Grynwald.ChangeLog.Test.Git
{
    /// <summary>
    /// Tests for <see cref="GitCommit"/>
    /// </summary>
    public class GitCommitTest : EqualityTest<GitCommit, GitCommitTest>, IEqualityTestDataProvider<GitCommit>
    {
        public IEnumerable<(GitCommit left, GitCommit right)> GetEqualTestCases()
        {
            yield return (
                new GitCommit(
                    TestGitIds.Id1,
                    "Commit Message",
                    new DateTime(2020, 01, 01),
                    new GitAuthor("user", "user@example.com")),
                new GitCommit(
                    TestGitIds.Id1,
                    "Commit Message",
                    new DateTime(2020, 01, 01),
                    new GitAuthor("user", "user@example.com"))
            );
        }

        public IEnumerable<(GitCommit left, GitCommit right)> GetUnequalTestCases()
        {
            yield return (
                new GitCommit(
                    TestGitIds.Id1,
                    "Commit Message",
                    new DateTime(2020, 01, 01),
                    new GitAuthor("user", "user@example.com")),
                new GitCommit(
                    TestGitIds.Id2,
                    "Commit Message",
                    new DateTime(2020, 01, 01),
                    new GitAuthor("user", "user@example.com"))
            );
            yield return (
                new GitCommit(
                    TestGitIds.Id1,
                    "Commit Message",
                    new DateTime(2020, 01, 01),
                    new GitAuthor("user", "user@example.com")),
                new GitCommit(
                    TestGitIds.Id1,
                    "Some other Commit Message",
                    new DateTime(2020, 01, 01),
                    new GitAuthor("user", "user@example.com"))
            );
            yield return (
                new GitCommit(
                    TestGitIds.Id1,
                    "Commit Message",
                    new DateTime(2020, 01, 01),
                    new GitAuthor("user", "user@example.com")),
                new GitCommit(
                    TestGitIds.Id1,
                    "Commit Message",
                    new DateTime(2019, 01, 01),
                    new GitAuthor("user", "user@example.com"))
            );
            yield return (
                new GitCommit(
                    TestGitIds.Id1,
                    "Commit Message",
                    new DateTime(2020, 01, 01),
                    new GitAuthor("user", "user@example.com")),
                new GitCommit(
                    TestGitIds.Id1,
                    "Commit Message",
                    new DateTime(2020, 01, 01),
                    new GitAuthor("user2", "user@example.net"))
            );
        }


        [Fact]
        public void Commit_id_must_not_be_null()
        {
            // ARRANGE
            var id = default(GitId);

            // ACT 
            var ex = Record.Exception(() => new GitCommit(id, "Some Message", DateTime.Now, new GitAuthor("user", "user@example.com")));

            // ASSERT
            Assert.NotNull(ex);
            var argumentException = Assert.IsType<ArgumentException>(ex);
            Assert.Equal("id", argumentException.ParamName);
        }

    }
}
