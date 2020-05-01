using System;
using System.Linq;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Templates.ViewModel;
using Xunit;

namespace Grynwald.ChangeLog.Test.Templates.ViewModel
{
    /// <summary>
    /// Tests for <see cref="SingleVersionChangeLogViewModel"/>
    /// </summary>
    public class SingleVersionChangeLogViewModelTest : TestBase
    {
        private readonly ChangeLogConfiguration m_DefaultConfiguration = ChangeLogConfigurationLoader.GetDefaultConfiguration();

        [Fact]
        public void Configuration_must_not_be_null()
        {
            Assert.Throws<ArgumentNullException>(() => new SingleVersionChangeLogViewModel(null!, GetSingleVersionChangeLog("1.2.3")));
        }

        [Fact]
        public void Model_must_not_be_null()
        {
            Assert.Throws<ArgumentNullException>(() => new SingleVersionChangeLogViewModel(m_DefaultConfiguration, null!));
        }

        [Theory]
        [InlineData("1.2.3", "1.2.3")]
        [InlineData("1.2", "1.2.0")]
        [InlineData("1.2.3.0", "1.2.3")]
        public void VersionDisplayName_is_the_normalized_version(string version, string expectedDisplayName)
        {
            // ARRANGE
            var sut = new SingleVersionChangeLogViewModel(m_DefaultConfiguration, GetSingleVersionChangeLog(version));

            // ACT / ASSERT
            Assert.Equal(expectedDisplayName, sut.VersionDisplayName);
        }

        [Fact]
        public void EntryGroups_contains_expected_groups()
        {
            // ARRANGE
            var model = GetSingleVersionChangeLog("1.2.3", entries: new[]
            {
                GetChangeLogEntry(type: "feat"),
                GetChangeLogEntry(type: "fix")
            });

            // ACT
            var sut = new SingleVersionChangeLogViewModel(m_DefaultConfiguration, model);

            // ASSERT
            Assert.NotNull(sut.EntryGroups);
            Assert.Equal(2, sut.EntryGroups.Count);
            Assert.Collection(sut.EntryGroups,
                group =>
                {
                    Assert.Equal("New Features", group.Title);
                    Assert.Single(group.Entries);
                },
                group =>
                {
                    Assert.Equal("Bug Fixes", group.Title);
                    Assert.Single(group.Entries);
                });
        }

        [Fact]
        public void EntryGroups_does_not_contain_emtpy_groups()
        {
            // ARRANGE
            var model = GetSingleVersionChangeLog("1.2.3", entries: new[]
            {
                GetChangeLogEntry(type: "feat"),
                GetChangeLogEntry(type: "feat")
            });

            // ACT
            var sut = new SingleVersionChangeLogViewModel(m_DefaultConfiguration, model);

            // ASSERT
            Assert.NotNull(sut.EntryGroups);
            Assert.Single(sut.EntryGroups);
            Assert.Collection(sut.EntryGroups,
                group =>
                {
                    Assert.Equal("New Features", group.Title);
                    Assert.Equal(2, group.Entries.Count);
                });
        }

        [Fact]
        public void EntryGroups_is_empty_if_changelog_is_empty()
        {
            // ARRANGE
            var model = GetSingleVersionChangeLog("1.2.3");

            // ACT
            var sut = new SingleVersionChangeLogViewModel(m_DefaultConfiguration, model);

            // ASSERT
            Assert.NotNull(sut.EntryGroups);
            Assert.Empty(sut.EntryGroups);
        }

        [Fact]
        public void Breaking_changes_is_empty_if_there_are_no_breaking_changes()
        {
            // ARRANGE
            var model = GetSingleVersionChangeLog("1.2.3", entries: new[]
            {
                GetChangeLogEntry(type: "feat"),
                GetChangeLogEntry(type: "feat")
            });

            // ACT
            var sut = new SingleVersionChangeLogViewModel(m_DefaultConfiguration, model);

            // ASSERT
            Assert.NotNull(sut.BreakingChanges);
            Assert.Empty(sut.BreakingChanges);
        }

        [Fact]
        public void Breaking_changes_includes_descriptions_for_all_entries()
        {
            // ARRANGE
            var model = GetSingleVersionChangeLog("1.2.3", entries: new[]
            {
                GetChangeLogEntry(type: "feat", summary: "Summary1", date: DateTime.Now.AddDays(-1), breakingChangeDescriptions: new []
                {
                    "description1",
                    "description2",
                }),
                GetChangeLogEntry(type: "feat"),
                // breaking changes must be included regardless of commit type
                GetChangeLogEntry(type: "refactor", date: DateTime.Now.AddDays(-2), breakingChangeDescriptions: new []
                {
                    "description3"
                }),
                // for changes marked as breaking changes but without a BREAKING CHANGE footer, the summary is included
                GetChangeLogEntry(type: "build", date: DateTime.Now.AddDays(1), scope: "SomeScope", summary: "Some Summary", isBreakingChange: true),
            });

            // ACT
            var sut = new SingleVersionChangeLogViewModel(m_DefaultConfiguration, model);

            // ASSERT
            Assert.NotNull(sut.BreakingChanges);
            Assert.Equal(4, sut.BreakingChanges.Count);
            // breaking changes must be sorted by commit date (oldest change first)
            Assert.Collection(sut.BreakingChanges,
                change =>
                {
                    Assert.Equal("description3", change.Description);
                },
                change =>
                {
                    Assert.Equal("description1", change.Description);
                    Assert.Same(sut.EntryGroups.Single(x => x.Title == "New Features").Entries.Single(x => x.Title == "Summary1"), change.Entry);
                },
                change =>
                {
                    Assert.Equal("description2", change.Description);
                    Assert.Same(sut.EntryGroups.Single(x => x.Title == "New Features").Entries.Single(x => x.Title == "Summary1"), change.Entry);
                },
                change =>
                {
                    Assert.Equal("SomeScope: Some Summary", change.Description);
                }
            );
        }
    }
}
