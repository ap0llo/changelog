using System;
using System.Collections.Generic;
using System.Linq;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Templates.ViewModel;
using Xunit;

namespace Grynwald.ChangeLog.Test.Templates.ViewModel
{
    public class SingleVersionChangeLogViewModelTest : TestBase
    {
        private readonly ChangeLogConfiguration m_DefaultConfiguration = ChangeLogConfigurationLoader.GetDefaultConfiguration();


        [Fact]
        public void EntryGroups_returns_entries_grouped_by_type_in_the_configured_order()
        {
            // ARRANGE
            var config = new ChangeLogConfiguration()
            {
                EntryTypes = new Dictionary<string, ChangeLogConfiguration.EntryTypeConfiguration>()
                {
                   { "feat", new ChangeLogConfiguration.EntryTypeConfiguration() {  DisplayName = "New Features", Priority = 100 } },
                   { "fix", new ChangeLogConfiguration.EntryTypeConfiguration() { DisplayName = "Bug Fixes", Priority = 90 } },
                   { "docs", new ChangeLogConfiguration.EntryTypeConfiguration() { DisplayName = "Documentation Updates", Priority = 80 } },
                   { "build", new ChangeLogConfiguration.EntryTypeConfiguration() { DisplayName = "Build System Changes", Priority = 70 } }
                }
            };
            var changelog = GetSingleVersionChangeLog("1.2.3", entries: new[]
            {
                GetChangeLogEntry(type: "feat"),
                GetChangeLogEntry(type: "fix"),
                GetChangeLogEntry(type: "feat"),
                GetChangeLogEntry(type: "refactor"),
                GetChangeLogEntry(type: "docs"),
            });

            var sut = new SingleVersionChangeLogViewModel(config, changelog);

            // ACT / ASSERT
            Assert.Collection(
                sut.EntryGroups,
                features =>
                {
                    Assert.Equal("New Features", features.DisplayName);
                    Assert.Equal(2, features.Entries.Count());
                },
                bugfixes =>
                {
                    Assert.Equal("Bug Fixes", bugfixes.DisplayName);
                    Assert.Single(bugfixes.Entries);
                },
                docsChanges =>
                {
                    Assert.Equal("Documentation Updates", docsChanges.DisplayName);
                    Assert.Single(docsChanges.Entries);
                },
                refactorings =>
                {
                    Assert.Equal("refactor", refactorings.DisplayName);
                    Assert.Single(refactorings.Entries);
                });
        }

        [Fact]
        public void EntryGroups_does_not_return_empty_groups()
        {
            // ARRANGE
            var changelog = GetSingleVersionChangeLog("1.2.3", entries: new[]
            {
                GetChangeLogEntry(type: "feat"),
                GetChangeLogEntry(type: "feat"),
                GetChangeLogEntry(type: "docs"),
                GetChangeLogEntry(type: "refactor"),
            });

            var sut = new SingleVersionChangeLogViewModel(m_DefaultConfiguration, changelog);

            // ACT / ASSERT
            Assert.All(sut.EntryGroups, g => Assert.NotEmpty(g.Entries));
        }

        [Fact]
        public void EntryGroups_have_expected_display_names()
        {
            // ARRANGE
            var config = new ChangeLogConfiguration()
            {
                EntryTypes = new Dictionary<string, ChangeLogConfiguration.EntryTypeConfiguration>()
                {
                    { "feat", new ChangeLogConfiguration.EntryTypeConfiguration() {  DisplayName = "New Features"} },
                    { "fix", new ChangeLogConfiguration.EntryTypeConfiguration() { DisplayName = null } },
                    { "docs", new ChangeLogConfiguration.EntryTypeConfiguration() { DisplayName = "" } },
                    { "build", new ChangeLogConfiguration.EntryTypeConfiguration() {  DisplayName = "\t" } },
                }
            };
            var changelog = GetSingleVersionChangeLog("1.2.3", entries: new[]
            {
                GetChangeLogEntry(type: "feat"),
                GetChangeLogEntry(type: "fix"),
                GetChangeLogEntry(type: "docs"),
                GetChangeLogEntry(type: "build"),
            });

            var sut = new SingleVersionChangeLogViewModel(config, changelog);

            // ACT / ASSERT
            // when no display name is configured, the change type is used as display name
            Assert.Collection(
                sut.EntryGroups,
                features => Assert.Equal("New Features", features.DisplayName),
                bugfixes => Assert.Equal("fix", bugfixes.DisplayName),
                docsChanges => Assert.Equal("docs", docsChanges.DisplayName),
                buildChanges => Assert.Equal("build", buildChanges.DisplayName));
        }

        [Fact]
        public void AllEntries_returns_entries_in_group_order()
        {
            // ARRANGE
            var config = new ChangeLogConfiguration()
            {
                EntryTypes = new Dictionary<string, ChangeLogConfiguration.EntryTypeConfiguration>()
                {
                    { "fix", new ChangeLogConfiguration.EntryTypeConfiguration() { Priority = 10 } },
                    { "feat", new ChangeLogConfiguration.EntryTypeConfiguration() { Priority = 5 } },
                    { "refactor", new ChangeLogConfiguration.EntryTypeConfiguration() { Priority = 20 } }
                }
            };

            var entries = new[]
            {
                GetChangeLogEntry(type: "fix"),
                GetChangeLogEntry(type: "feat"),
                GetChangeLogEntry(type: "feat"),
                GetChangeLogEntry(type: "fix"),
                GetChangeLogEntry(type: "refactor"),
            };

            var expectedOrder = new[] { entries[4], entries[0], entries[3], entries[1], entries[2] };

            var changelog = GetSingleVersionChangeLog("1.2.3", entries: entries);

            var sut = new SingleVersionChangeLogViewModel(config, changelog);

            // ACT / ASSERT
            var assertions = expectedOrder.Select<ChangeLogEntry, Action<ChangeLogEntryViewModel>>(
                expected => actual =>
                {
                    Assert.Equal(expected.Type, actual.Type);
                    Assert.Equal(expected.Commit, actual.Commit);
                }
            ).ToArray();

            Assert.Collection(sut.AllEntries, assertions);
        }
    }
}

