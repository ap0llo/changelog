using System;
using System.Collections.Generic;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Model;
using NuGet.Versioning;
using Xunit;

namespace Grynwald.ChangeLog.Test.Model
{
    /// <summary>
    /// Tests for <see cref="VersionInfo"/>
    /// </summary>
    public class VersionInfoTest : EqualityTest<VersionInfo, VersionInfoTest>, IEqualityTestDataProvider<VersionInfo>
    {
        [Fact]
        public void Constructor_checks_arguments()
        {
            Assert.Throws<ArgumentNullException>(() => new VersionInfo(null!, TestGitIds.Id1));
            Assert.Throws<ArgumentException>(() => new VersionInfo(NuGetVersion.Parse("1.2.3"), default));
        }

        [Fact]
        public void Constructor_initializes_properties()
        {
            // ARRANGE
            var version = NuGetVersion.Parse("1.2.3-alpha");
            var commit = TestGitIds.Id1;

            // ACT 
            var sut = new VersionInfo(version, commit);

            // ASSERT
            Assert.Equal(version, sut.Version);
            Assert.Equal(commit, sut.Commit);
        }

        public IEnumerable<(VersionInfo left, VersionInfo right)> GetEqualTestCases()
        {
            yield return (
                new VersionInfo(NuGetVersion.Parse("1.2.3"), TestGitIds.Id1),
                new VersionInfo(NuGetVersion.Parse("1.2.3"), TestGitIds.Id1)
            );

            yield return (
                new VersionInfo(NuGetVersion.Parse("1.2.3"), TestGitIds.Id1),
                new VersionInfo(NuGetVersion.Parse("1.2.3"), TestGitIds.Id1)
            );
        }

        public IEnumerable<(VersionInfo left, VersionInfo right)> GetUnequalTestCases()
        {
            yield return (
                new VersionInfo(NuGetVersion.Parse("1.2.3"), TestGitIds.Id1),
                new VersionInfo(NuGetVersion.Parse("4.5.6"), TestGitIds.Id1)
            );

            yield return (
                new VersionInfo(NuGetVersion.Parse("1.2.3"), TestGitIds.Id1),
                new VersionInfo(NuGetVersion.Parse("1.2.3"), TestGitIds.Id2)
            );
        }
    }
}
