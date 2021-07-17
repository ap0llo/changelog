using System.Linq;
using System.Threading.Tasks;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.ChangeLog.Filtering;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Pipeline;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Grynwald.ChangeLog.Test.Filtering
{
    /// <summary>
    /// Unit tests for <see cref="FilterEntriesTask"/>
    /// </summary>
    public class FilterEntriesTaskTest : TestBase
    {
        private readonly ILogger<FilterEntriesTask> m_Logger;
        private readonly ChangeLogConfiguration m_DefaultConfiguration;


        public FilterEntriesTaskTest(ITestOutputHelper testOutputHelper)
        {
            m_Logger = new XunitLogger<FilterEntriesTask>(testOutputHelper);
            m_DefaultConfiguration = ChangeLogConfigurationLoader.GetDefaultConfiguration();
        }


        [Fact]
        public async Task RunAsync_removes_all_entries_except_features_and_bugfixes_when_using_the_default_configuration()
        {
            // ARRANGE
            var changeLog = GetSingleVersionChangeLog("1.2.3", entries: new[]
            {
                GetChangeLogEntry(type: "feat"),
                GetChangeLogEntry(type: "fix"),
                GetChangeLogEntry(type: "refactor"),
                GetChangeLogEntry(type: "build"),
            });

            var sut = new FilterEntriesTask(m_Logger, m_DefaultConfiguration);

            // ACT
            var result = await sut.RunAsync(new ApplicationChangeLog() { changeLog });

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);
            Assert.Equal(2, changeLog.AllEntries.Count());
            Assert.All(changeLog.AllEntries, e => Assert.True(e.Type == CommitType.Feature || e.Type == CommitType.BugFix));
        }

        [Fact]
        public async Task RunAsync_removes_entries_as_according_to_the_filter_configuration()
        {
            // ARRANGE
            var config = new ChangeLogConfiguration()
            {
                Filter = new ChangeLogConfiguration.FilterConfiguration()
                {
                    Include = new[]
                    {
                        new ChangeLogConfiguration.FilterExpressionConfiguration() { Type = "build" },
                        new ChangeLogConfiguration.FilterExpressionConfiguration() { Type = "refactor" }
                    },
                    Exclude = new[]
                    {
                        new ChangeLogConfiguration.FilterExpressionConfiguration() { Type = "build", Scope = "ignored-scope" }
                    },
                }
            };
            var changeLog = GetSingleVersionChangeLog("1.2.3", entries: new[]
            {
                GetChangeLogEntry(type: "feat"),
                GetChangeLogEntry(type: "fix"),
                GetChangeLogEntry(type: "refactor"),
                GetChangeLogEntry(type: "build", scope: "ignored-scope"),
                GetChangeLogEntry(type: "build"),
            });

            var sut = new FilterEntriesTask(m_Logger, config);

            // ACT
            var result = await sut.RunAsync(new ApplicationChangeLog() { changeLog });

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);
            Assert.Equal(2, changeLog.AllEntries.Count());
            Assert.All(changeLog.AllEntries, e =>
            {
                Assert.True(e.Type == new CommitType("build") || e.Type == new CommitType("refactor"));
                Assert.Null(e.Scope);
            });
        }

        [Fact]
        public async Task RunAsync_does_not_remote_entries_if_they_contain_breaking_changes()
        {
            // ARRANGE
            var changeLog = GetSingleVersionChangeLog("1.2.3", entries: new[]
            {
                GetChangeLogEntry(type: "refactor", isBreakingChange: true),
                GetChangeLogEntry(type: "build", breakingChangeDescriptions: new[] { "Some breaking change" }),
            });

            var sut = new FilterEntriesTask(m_Logger, m_DefaultConfiguration);

            // ACT
            var result = await sut.RunAsync(new ApplicationChangeLog() { changeLog });

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);
            Assert.Equal(2, changeLog.AllEntries.Count());
        }
    }
}
