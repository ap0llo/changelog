using System;
using System.Linq;
using Grynwald.ChangeLog.Model;
using NuGet.Versioning;
using Xunit;

namespace Grynwald.ChangeLog.Test.Model
{
    public class ApplicationChangeLogTest : TestBase
    {
        [Fact]
        public void ChangeLogs_are_returned_ordered_by_version()
        {
            // ARRANGE
            var item1 = GetSingleVersionChangeLog("1.0.1");
            var item2 = GetSingleVersionChangeLog("3.5.0");
            var item3 = GetSingleVersionChangeLog("2.0.7");

            var expectedOrdered = new[] { item2, item3, item1 };

            var sut = new ApplicationChangeLog() { item1, item2, item3 };

            // ACT
            var actualOrdered1 = sut.ChangeLogs.ToArray();
            var actualOrdered2 = sut.ToArray();

            // ASSERT
            Assert.Equal(expectedOrdered, actualOrdered1);
            Assert.Equal(expectedOrdered, actualOrdered2);
        }

        [Fact]
        public void ContainsVersion_returns_expected_value()
        {
            // ARRANGE
            var versionInfo = GetSingleVersionChangeLog("4.5.6");

            var sut = new ApplicationChangeLog() { versionInfo };

            // ACT 
            var contains1 = sut.ContainsVersion(NuGetVersion.Parse("4.5.6"));
            var contains2 = sut.ContainsVersion(NuGetVersion.Parse("1.2"));

            // ASSERT
            Assert.True(contains1);
            Assert.False(contains2);
        }

        [Fact]
        public void Add_throws_InvalidOperationException_if_version_already_exists()
        {
            // ARRANGE
            var versionInfo1 = GetSingleVersionChangeLog("4.5.6", "abc123");
            var versionInfo2 = GetSingleVersionChangeLog("4.5.6", "def456");

            var sut = new ApplicationChangeLog() { versionInfo1 };

            // ACT / ASSERT
            Assert.Throws<InvalidOperationException>(() => sut.Add(versionInfo2));
        }
    }
}
