using System;
using System.Linq;
using System.Threading.Tasks;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Pipeline;
using Grynwald.ChangeLog.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Grynwald.ChangeLog.Test.Tasks
{
    /// <summary>
    /// Unit tests for <see cref="ParseCommitsTask"/>
    /// </summary>
    public class ParseCommitsTaskTest : TestBase
    {
        private readonly ILogger<ParseCommitsTask> m_Logger;
        private readonly ChangeLogConfiguration m_DefaultConfiguration;


        public ParseCommitsTaskTest(ITestOutputHelper testOutputHelper)
        {
            m_Logger = new XunitLogger<ParseCommitsTask>(testOutputHelper);
            m_DefaultConfiguration = ChangeLogConfigurationLoader.GetDefaultConfiguration();
        }

        [Fact]
        public void Logger_must_not_be_null()
        {
            // ARRANGE

            // ACT 
            var ex = Record.Exception(() => new ParseCommitsTask(logger: null!, configuration: m_DefaultConfiguration));

            // ASSERT
            var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
            Assert.Equal("logger", argumentNullException.ParamName);
        }

        [Fact]
        public void Configuration_must_not_be_null()
        {
            // ARRANGE

            // ACT 
            var ex = Record.Exception(() => new ParseCommitsTask(logger: m_Logger, configuration: null!));

            // ASSERT
            var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
            Assert.Equal("configuration", argumentNullException.ParamName);
        }


        [Fact]
        public async Task Run_does_nothing_for_empty_changelog()
        {
            // ARRANGE            
            var sut = new ParseCommitsTask(m_Logger, m_DefaultConfiguration);

            // ACT
            var changelog = new ApplicationChangeLog();
            var result = await sut.RunAsync(changelog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Skipped, result);
        }

        [Fact]
        public async Task Run_adds_all_parsable_changes_if_no_previous_version_exists()
        {
            // ARRANGE                        
            var sut = new ParseCommitsTask(m_Logger, m_DefaultConfiguration);

            var versionChangeLog = GetSingleVersionChangeLog("1.2.3", TestGitIds.Id1);
            versionChangeLog.Add(GetGitCommit(TestGitIds.Id1, "feat: Some new feature"));
            versionChangeLog.Add(GetGitCommit(TestGitIds.Id2, "fix: Some bugfix"));

            var changelog = new ApplicationChangeLog() { versionChangeLog };

            // ACT
            var result = await sut.RunAsync(changelog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);

            Assert.NotNull(versionChangeLog.AllEntries);
            Assert.Equal(2, versionChangeLog.AllEntries.Count());

            {
                var entry = Assert.Single(versionChangeLog.AllEntries, e => e.Type.Equals(CommitType.Feature));
                Assert.Equal(TestGitIds.Id1, entry.Commit);
                Assert.Null(entry.Scope);
                Assert.Equal("Some new feature", entry.Summary);
                Assert.NotNull(entry.Body);
                Assert.Empty(entry.Body);
            }
            {
                var entry = Assert.Single(versionChangeLog.AllEntries, e => e.Type.Equals(CommitType.BugFix));
                Assert.Equal(TestGitIds.Id2, entry.Commit);
                Assert.Null(entry.Scope);
                Assert.Equal("Some bugfix", entry.Summary);
                Assert.NotNull(entry.Body);
                Assert.Empty(entry.Body);
            }
        }

        [Fact]
        public async Task Run_adds_the_expected_entries_if_a_previous_version_exists()
        {
            // ARRANGE
            var sut = new ParseCommitsTask(m_Logger, m_DefaultConfiguration);

            var versionChangeLog1 = GetSingleVersionChangeLog("1.2.3", TestGitIds.Id1);

            var versionChangeLog2 = GetSingleVersionChangeLog("2.4.5", TestGitIds.Id2);
            versionChangeLog2.Add(GetGitCommit(TestGitIds.Id1, "feat: Some new feature"));
            versionChangeLog2.Add(GetGitCommit(TestGitIds.Id2, "fix: Some bugfix"));

            var changelog = new ApplicationChangeLog()
            {
                versionChangeLog1, versionChangeLog2
            };

            // ACT
            var result = await sut.RunAsync(changelog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);

            Assert.NotNull(versionChangeLog1.AllEntries);
            Assert.Empty(versionChangeLog1.AllEntries);

            Assert.NotNull(versionChangeLog2.AllEntries);
            Assert.Equal(2, versionChangeLog2.AllEntries.Count());

            {
                var entry = Assert.Single(versionChangeLog2.AllEntries, e => e.Type.Equals(CommitType.Feature));
                Assert.Equal(TestGitIds.Id1, entry.Commit);
                Assert.Null(entry.Scope);
                Assert.Equal("Some new feature", entry.Summary);
                Assert.NotNull(entry.Body);
                Assert.Empty(entry.Body);
            }
            {
                var entry = Assert.Single(versionChangeLog2.AllEntries, e => e.Type.Equals(CommitType.BugFix));
                Assert.Equal(TestGitIds.Id2, entry.Commit);
                Assert.Null(entry.Scope);
                Assert.Equal("Some bugfix", entry.Summary);
                Assert.NotNull(entry.Body);
                Assert.Empty(entry.Body);
            }
        }

        [Fact]
        public async Task Run_ignores_unparsable_commit_messages()
        {
            // ARRANGE            
            var sut = new ParseCommitsTask(m_Logger, m_DefaultConfiguration);

            var versionChangeLog = GetSingleVersionChangeLog("1.2.3", TestGitIds.Id1);
            versionChangeLog.Add(GetGitCommit(commitMessage: "Not a conventional commit"));
            var changelog = new ApplicationChangeLog() { versionChangeLog };

            // ACT
            var result = await sut.RunAsync(changelog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);
            Assert.NotNull(versionChangeLog.AllEntries);
            Assert.Empty(versionChangeLog.AllEntries);
        }

        [Fact]
        public async Task Run_uses_configured_parser_setting_01()
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Parser.Mode = ChangeLogConfiguration.ParserMode.Loose;

            var sut = new ParseCommitsTask(m_Logger, config);

            var versionChangeLog = GetSingleVersionChangeLog("1.2.3", TestGitIds.Id1);
            versionChangeLog.Add(GetGitCommit(commitMessage: "feat: Some Description\r\n" + "\r\n" + "\r\n" + "Message Body\r\n"));
            var changelog = new ApplicationChangeLog() { versionChangeLog };

            // ACT
            var result = await sut.RunAsync(changelog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);
            Assert.NotNull(versionChangeLog.AllEntries);
            Assert.Single(versionChangeLog.AllEntries);
        }

        [Fact]
        public async Task Run_uses_configured_parser_setting_02()
        {
            // ARRANGE
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Parser.Mode = ChangeLogConfiguration.ParserMode.Strict;

            var sut = new ParseCommitsTask(m_Logger, config);

            var versionChangeLog = GetSingleVersionChangeLog("1.2.3", TestGitIds.Id1);
            versionChangeLog.Add(GetGitCommit(commitMessage: "feat: Some Description\r\n" + "\r\n" + "\r\n" + "Message Body\r\n"));
            var changelog = new ApplicationChangeLog() { versionChangeLog };

            // ACT
            var result = await sut.RunAsync(changelog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);
            Assert.NotNull(versionChangeLog.AllEntries);
            Assert.Empty(versionChangeLog.AllEntries);
        }

        //TODO: Scope, footers, body
    }
}
