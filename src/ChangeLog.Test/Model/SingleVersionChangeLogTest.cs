using System;
using System.Linq;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Model;
using Xunit;

namespace Grynwald.ChangeLog.Test.Model
{
    /// <summary>
    /// Tests for <see cref="SingleVersionChangeLog"/>
    /// </summary>
    public class SingleVersionChangeLogTest : TestBase
    {
        [Fact]
        public void Constructor_checks_arguments_for_null()
        {
            Assert.Throws<ArgumentNullException>(() => new SingleVersionChangeLog(null!));
        }

        [Fact]
        public void Entries_are_returned_sorted_by_date()
        {
            // ARRANGE
            var entry1 = GetChangeLogEntry(date: new DateTime(2020, 1, 10), type: "feat");
            var entry2 = GetChangeLogEntry(date: new DateTime(2020, 1, 20), type: "feat");
            var entry3 = GetChangeLogEntry(date: new DateTime(2020, 1, 1), type: "fix");
            var entry4 = GetChangeLogEntry(date: new DateTime(2020, 1, 3), type: "fix", isBreakingChange: true);
            var entry5 = GetChangeLogEntry(date: new DateTime(2020, 1, 30), type: "refactor", isBreakingChange: true);

            var sut = GetSingleVersionChangeLog("1.2.3");
            sut.Add(entry1);
            sut.Add(entry2);
            sut.Add(entry3);
            sut.Add(entry4);
            sut.Add(entry5);

            var expectedOrdered = new[] { entry3, entry4, entry1, entry2, entry5 };
            var expectedOrderedBreakingChanges = new[] { entry4, entry5 };

            // ACT 
            var actualOrdered1 = sut.AllEntries.ToArray();
            var actualOrdered2 = sut.ToArray();
            var actualOrderedBreakingChanges = sut.BreakingChanges.ToArray();

            // ASSERT
            Assert.Equal(expectedOrdered, actualOrdered1);
            Assert.Equal(expectedOrdered, actualOrdered2);
            Assert.Equal(expectedOrderedBreakingChanges, actualOrderedBreakingChanges);
        }

        [Fact]
        public void BreakingChanges_returns_expected_entries()
        {
            // ARRANGE
            var sut = GetSingleVersionChangeLog("1.2.3", entries: new[]
            {
                GetChangeLogEntry(type: "feat", isBreakingChange: true),
                GetChangeLogEntry(type: "fix"),
                GetChangeLogEntry(type: "feat"),
                GetChangeLogEntry(type: "build", breakingChangeDescriptions: new[]{ "Some breaking change" }),
                GetChangeLogEntry(type: "refactor"),
            });

            // ACT
            var breakingChanges = sut.BreakingChanges;

            // ASSERT
            Assert.NotNull(breakingChanges);
            Assert.Equal(2, breakingChanges.Count());
            Assert.Collection(
                breakingChanges,
                e => Assert.Equal(CommitType.Feature, e.Type),
                e => Assert.Equal(new CommitType("build"), e.Type));
        }

        [Fact]
        public void Add_checks_ChangeLogEntry_argument_for_null()
        {
            var sut = GetSingleVersionChangeLog("1.2.3");
            Assert.Throws<ArgumentNullException>(() => sut.Add((ChangeLogEntry)null!));
        }

        [Fact]
        public void Add_checks_GitCommit_argument_for_null()
        {
            var sut = GetSingleVersionChangeLog("1.2.3");
            Assert.Throws<ArgumentNullException>(() => sut.Add((GitCommit)null!));
        }

        [Fact]
        public void Add_throws_InvalidOperationException_when_commit_already_exists()
        {
            // ARRANGE
            var commit = GetGitCommit(TestGitIds.Id1);
            var sut = GetSingleVersionChangeLog("1.2.3");
            sut.Add(commit);

            // ACT 
            var ex = Record.Exception(() => sut.Add(commit));

            // ASSERT
            Assert.IsType<InvalidOperationException>(ex);
            Assert.Contains("already contains commit", ex.Message);
        }

        [Fact]
        public void Remove_checks_argument_for_null()
        {
            var sut = GetSingleVersionChangeLog("1.2.3");
            Assert.Throws<ArgumentNullException>(() => sut.Remove(null!));
        }
    }
}
