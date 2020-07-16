using System;
using System.Linq;
using Grynwald.ChangeLog.ConventionalCommits;
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
            var expectedOrderedFeatures = new[] { entry1, entry2 };
            var expectedOrderedBugFixes = new[] { entry3, entry4 };
            var expectedOrderedBreakingChanges = new[] { entry4, entry5 };

            // ACT 
            var actualOrdered1 = sut.AllEntries.ToArray();
            var actualOrdered2 = sut.ToArray();
            var actualOrderedFeatures = sut[CommitType.Feature].Entries.ToArray();
            var actualOrderedBugFixes = sut[CommitType.BugFix].Entries.ToArray();
            var actualOrderedBreakingChanges = sut.BreakingChanges.ToArray();

            // ASSERT
            Assert.Equal(expectedOrdered, actualOrdered1);
            Assert.Equal(expectedOrdered, actualOrdered2);
            Assert.Equal(expectedOrderedFeatures, actualOrderedFeatures);
            Assert.Equal(expectedOrderedBugFixes, actualOrderedBugFixes);
            Assert.Equal(expectedOrderedBreakingChanges, actualOrderedBreakingChanges);
        }

        [Fact]
        public void Indexer_returns_expected_entry_group_01()
        {
            // ARRANGE
            var sut = GetSingleVersionChangeLog("1.2.3", entries: new[]
            {
                GetChangeLogEntry(type: "feat"),
                GetChangeLogEntry(type: "fix")
            });

            var type = new CommitType("build");

            // ACT
            var group = sut[type];

            // ASSERT
            Assert.NotNull(group);
            Assert.Equal(type, group.Type);
            Assert.Empty(group.Entries);
        }

        [Fact]
        public void Indexer_returns_expected_entry_group_02()
        {
            // ARRANGE
            var sut = GetSingleVersionChangeLog("1.2.3", entries: new[]
            {
                GetChangeLogEntry(type: "feat"),
                GetChangeLogEntry(type: "fix"),
                GetChangeLogEntry(type: "feat"),
                GetChangeLogEntry(type: "build"),
                GetChangeLogEntry(type: "refactor"),
            });

            // ACT
            var features = sut[CommitType.Feature];
            var bugFixes = sut[CommitType.BugFix];
            var buildChanges = sut[new CommitType("build")];


            // ASSERT
            Assert.NotNull(features);
            Assert.Equal(CommitType.Feature, features.Type);
            Assert.Equal(2, features.Entries.Count);

            Assert.NotNull(bugFixes);
            Assert.Equal(CommitType.BugFix, bugFixes.Type);
            Assert.Single(bugFixes.Entries);

            Assert.NotNull(buildChanges);
            Assert.Equal(new CommitType("build"), buildChanges.Type);
            Assert.Single(buildChanges.Entries);
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
        public void Add_checks_argument_for_null()
        {
            var sut = GetSingleVersionChangeLog("1.2.3");
            Assert.Throws<ArgumentNullException>(() => sut.Add(null!));
        }

        [Fact]
        public void Remove_checks_argument_for_null()
        {
            var sut = GetSingleVersionChangeLog("1.2.3");
            Assert.Throws<ArgumentNullException>(() => sut.Remove(null!));
        }
    }
}
