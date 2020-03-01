using System;
using ChangeLogCreator.ConventionalCommits;
using ChangeLogCreator.Git;
using ChangeLogCreator.Model;
using ChangeLogCreator.Tasks;
using Moq;
using NuGet.Versioning;
using Xunit;

namespace ChangeLogCreator.Test.Tasks
{
    public class ParseCommitsTaskTest
    {
        private SingleVersionChangeLog GetSingleVersionChangeLog(string version, string commitId)
        {
            return new SingleVersionChangeLog(
                new VersionInfo(
                    SemanticVersion.Parse(version),
                    new GitId(commitId)
                    ));
        }

        private GitCommit GetGitCommit(string? id = null, string? commitMessage = null)
        {
            return new GitCommit(
                id: new GitId(id ?? "0000"),
                commitMessage: commitMessage ?? "",
                date: new DateTime(),
                author: new GitAuthor("Someone", "someone@example.com")
            );

        }


        [Fact]
        public void Run_does_nothing_for_empty_changelog()
        {
            // ARRANGE
            var repo = Mock.Of<IGitRepository>(MockBehavior.Strict);

            var sut = new ParseCommitsTask(repo);

            // ACT
            var changelog = new ChangeLog();
            sut.Run(changelog);

            // ASSERT
        }

        [Fact]
        public void Run_adds_all_parsable_changes_if_no_previous_version_exists()
        {
            // ARRANGE
            var repo = new Mock<IGitRepository>(MockBehavior.Strict);
            repo
                .Setup(x => x.GetCommits(null, It.IsAny<GitId>()))
                .Returns(new[]
                {
                    GetGitCommit("01", "feat: Some new feature"),
                    GetGitCommit("02", "fix: Some bugfix")
                });

            var sut = new ParseCommitsTask(repo.Object);

            var versionChangeLog = GetSingleVersionChangeLog("1.2.3", "01");
            var changelog = new ChangeLog() { versionChangeLog };

            // ACT
            sut.Run(changelog);

            // ASSERT
            repo.Verify(x => x.GetCommits(It.IsAny<GitId?>(), It.IsAny<GitId>()), Times.Once);

            Assert.NotNull(versionChangeLog.Entries);
            Assert.Equal(2, versionChangeLog.Entries.Count);

            {
                var entry = Assert.Single(versionChangeLog.Entries, e => e.Type.Equals(CommitType.Feature));
                Assert.Equal(new GitId("01"), entry.Commit);
                Assert.Null(entry.Scope);
                Assert.Equal("Some new feature", entry.Summary);
                Assert.NotNull(entry.Body);
                Assert.Empty(entry.Body);
            }
            {
                var entry = Assert.Single(versionChangeLog.Entries, e => e.Type.Equals(CommitType.BugFix));
                Assert.Equal(new GitId("02"), entry.Commit);
                Assert.Null(entry.Scope);
                Assert.Equal("Some bugfix", entry.Summary);
                Assert.NotNull(entry.Body);
                Assert.Empty(entry.Body);
            }
        }

        [Fact]
        public void Run_adds_the_expected_entries_if_a_previous_version_exists()
        {
            // ARRANGE
            var repo = new Mock<IGitRepository>(MockBehavior.Strict);
            repo
                .Setup(x => x.GetCommits(null, new GitId("01")))
                .Returns(Array.Empty<GitCommit>());

            repo
                .Setup(x => x.GetCommits(new GitId("01"), new GitId("02")))
                .Returns(new[]
                {
                    GetGitCommit("ab", "feat: Some new feature" ),
                    GetGitCommit("cd", "fix: Some bugfix" ),
                });

            var sut = new ParseCommitsTask(repo.Object);

            var versionChangeLog1 = GetSingleVersionChangeLog("1.2.3", "01");
            var versionChangeLog2 = GetSingleVersionChangeLog("2.4.5", "02");
            var changelog = new ChangeLog()
            {
                versionChangeLog1, versionChangeLog2
            };

            // ACT
            sut.Run(changelog);

            // ASSERT
            repo.Verify(x => x.GetCommits(null, It.IsAny<GitId>()), Times.Once);
            repo.Verify(x => x.GetCommits(It.IsAny<GitId>(), It.IsAny<GitId>()), Times.Once);

            Assert.NotNull(versionChangeLog1.Entries);
            Assert.Empty(versionChangeLog1.Entries);

            Assert.NotNull(versionChangeLog2.Entries);
            Assert.Equal(2, versionChangeLog2.Entries.Count);

            {
                var entry = Assert.Single(versionChangeLog2.Entries, e => e.Type.Equals(CommitType.Feature));
                Assert.Equal(new GitId("ab"), entry.Commit);
                Assert.Null(entry.Scope);
                Assert.Equal("Some new feature", entry.Summary);
                Assert.NotNull(entry.Body);
                Assert.Empty(entry.Body);
            }
            {
                var entry = Assert.Single(versionChangeLog2.Entries, e => e.Type.Equals(CommitType.BugFix));
                Assert.Equal(new GitId("cd"), entry.Commit);
                Assert.Null(entry.Scope);
                Assert.Equal("Some bugfix", entry.Summary);
                Assert.NotNull(entry.Body);
                Assert.Empty(entry.Body);
            }
        }

        //TODO: gets expected range

        [Fact]
        public void Run_ignores_unparsable_commit_messages()

        {
            // ARRANGE
            var repo = new Mock<IGitRepository>(MockBehavior.Strict);
            repo
                .Setup(x => x.GetCommits(null, It.IsAny<GitId>()))
                .Returns(new[]
                {
                    GetGitCommit(commitMessage: "Not a conventional commit"),
                });

            var sut = new ParseCommitsTask(repo.Object);

            var versionChangeLog = GetSingleVersionChangeLog("1.2.3", "01");
            var changelog = new ChangeLog() { versionChangeLog };

            // ACT
            sut.Run(changelog);

            // ASSERT
            Assert.NotNull(versionChangeLog.Entries);
            Assert.Empty(versionChangeLog.Entries);
        }

        //TODO: Scope, footers, body


    }
}
