using System.Linq;
using System.Threading.Tasks;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Grynwald.ChangeLog.Test.Tasks
{
    public class FilterVersionsTaskTest : TestBase
    {
        private readonly ILogger<FilterVersionsTask> m_Logger = NullLogger<FilterVersionsTask>.Instance;

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task Run_has_no_effect_when_the_version_range_setting_is_null_or_empty(string versionRange)
        {
            // ARRANGE
            var config = new ChangeLogConfiguration() { VersionRange = versionRange };

            var sut = new FilterVersionsTask(m_Logger, config);

            var changeLog = new ApplicationChangeLog()
            {
                GetSingleVersionChangeLog("1.2.3", null),
                GetSingleVersionChangeLog("4.5.6", null)
            };

            // ACT 
            await sut.RunAsync(changeLog);

            // ASSERT
            Assert.Equal(2, changeLog.ChangeLogs.Count());
        }


        [Theory]
        [InlineData("not-a-range")]
        [InlineData("(1.0")]
        public async Task Run_has_no_effect_when_the_version_range_setting_is_not_a_version_range(string versionRange)
        {
            // ARRANGE
            var config = new ChangeLogConfiguration() { VersionRange = versionRange };

            var sut = new FilterVersionsTask(m_Logger, config);

            var changeLog = new ApplicationChangeLog()
            {
                GetSingleVersionChangeLog("1.2.3", null),
                GetSingleVersionChangeLog("4.5.6", null)
            };

            // ACT 
            await sut.RunAsync(changeLog);

            // ASSERT
            Assert.Equal(2, changeLog.ChangeLogs.Count());
        }

        [Theory]
        [InlineData("[2.0,)")]
        [InlineData("[4.5.6]")]
        public async Task Run_removes_the_expected_changelog_entries(string versionRange)
        {
            // ARRANGE
            var config = new ChangeLogConfiguration() { VersionRange = versionRange };

            var sut = new FilterVersionsTask(m_Logger, config);

            var version1ChangeLog = GetSingleVersionChangeLog("1.2.3", null);
            var version2ChangeLog = GetSingleVersionChangeLog("4.5.6", null);

            var changeLog = new ApplicationChangeLog()
            {
                version1ChangeLog,
                version2ChangeLog
            };

            // ACT 
            await sut.RunAsync(changeLog);

            // ASSERT
            var remainingEntry = Assert.Single(changeLog.ChangeLogs);
            Assert.Equal(version2ChangeLog, remainingEntry);
        }
    }
}
