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
        public void EntryGroups_returns_configured_groups()
        {
            // ARRANGE
            var config = new ChangeLogConfiguration()
            {
                EntryTypes = new Dictionary<string, ChangeLogConfiguration.EntryTypeConfiguration>()
                {
                   { "feat", new ChangeLogConfiguration.EntryTypeConfiguration() {  DisplayName = "New Features" } },
                   { "fix", new ChangeLogConfiguration.EntryTypeConfiguration() { DisplayName = "Bug Fixes" } },
                   { "docs", new ChangeLogConfiguration.EntryTypeConfiguration() { DisplayName = "Documentation Updates" } },
                   { "build", new ChangeLogConfiguration.EntryTypeConfiguration() { DisplayName = "Build System Changes" } },
                }
            };
            var changelog = GetSingleVersionChangeLog("1.2.3", entries: new[]
            {
                GetChangeLogEntry(type: "feat"),
                GetChangeLogEntry(type: "feat"),
                GetChangeLogEntry(type: "docs"),
                GetChangeLogEntry(type: "refactor"),
                GetChangeLogEntry(type: "fix"),
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
            var entries = new[]
            {
                GetChangeLogEntry(type: "fix"),
                GetChangeLogEntry(type: "feat"),
                GetChangeLogEntry(type: "feat"),
                GetChangeLogEntry(type: "fix"),
                GetChangeLogEntry(type: "refactor"),
            };

            var expectedOrder = new[] { entries[1], entries[2], entries[0], entries[3] };

            var changelog = GetSingleVersionChangeLog("1.2.3", entries: entries);

            var sut = new SingleVersionChangeLogViewModel(m_DefaultConfiguration, changelog);

            // ACT / ASSERT
            var assertions = expectedOrder.Select<ChangeLogEntry, Action<ChangeLogEntry>>(
                expected => actual => Assert.Same(expected, actual)
            ).ToArray();

            Assert.Collection(sut.AllEntries, assertions);
        }

        [Fact]
        public void AllEntries_returns_changes_of_all_configured_types()
        {
            // ARRANGE
            var config = new ChangeLogConfiguration()
            {
                EntryTypes = new Dictionary<string, ChangeLogConfiguration.EntryTypeConfiguration>()
                {
                    { "feat", new ChangeLogConfiguration.EntryTypeConfiguration() { DisplayName = "New Features" } },
                    { "fix",  new ChangeLogConfiguration.EntryTypeConfiguration() { DisplayName = "Bug Fixes" } },
                    { "docs",  new ChangeLogConfiguration.EntryTypeConfiguration() { DisplayName = "Documentation Updates" } },
                    { "build", new ChangeLogConfiguration.EntryTypeConfiguration() { DisplayName = "Build System Changes" } },
                }
            };
            var entries = new[]
            {
                GetChangeLogEntry(type: "feat"),
                GetChangeLogEntry(type: "feat"),
                GetChangeLogEntry(type: "docs"),
                GetChangeLogEntry(type: "refactor"),
                GetChangeLogEntry(type: "fix"),
            };

            var expectedOrder = new[] { entries[0], entries[1], entries[4], entries[2] };

            var sut = new SingleVersionChangeLogViewModel(config, GetSingleVersionChangeLog("1.2.3", entries: entries));

            // ACT / ASSERT
            var assertions = expectedOrder.Select<ChangeLogEntry, Action<ChangeLogEntry>>(
                expected => actual => Assert.Same(expected, actual)
            ).ToArray();

            Assert.Collection(sut.AllEntries, assertions);
        }
    }
}

