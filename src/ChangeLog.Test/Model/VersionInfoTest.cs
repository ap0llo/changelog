using System;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Model;
using NuGet.Versioning;
using Xunit;

namespace Grynwald.ChangeLog.Test.Model
{
    /// <summary>
    /// Tests for <see cref="VersionInfo"/>
    /// </summary>
    public class VersionInfoTest
    {
        [Fact]
        public void Constructor_checks_arguments_for_null()
        {
            Assert.Throws<ArgumentNullException>(() => new VersionInfo(null!, new GitId("123abc")));
        }

        [Fact]
        public void Constructor_initializes_properties()
        {
            // ARRANGE
            var version = NuGetVersion.Parse("1.2.3-alpha");
            var commit = new GitId("123abc");

            // ACT 
            var sut = new VersionInfo(version, commit);

            // ASSERT
            Assert.Equal(version, sut.Version);
            Assert.Equal(commit, sut.Commit);
        }

        [Fact]
        public void Two_VersionInfo_instances_are_equal_when_the_version_and_commit_are_equal()
        {
            var commit1 = new GitId("abc123");
            var commit2 = new GitId("ABC123");
            var version = NuGetVersion.Parse("1.2.3-alpha");

            var versionInfo1 = new VersionInfo(version, commit1);
            var versionInfo2 = new VersionInfo(version, commit2);

            Assert.Equal(versionInfo1.GetHashCode(), versionInfo2.GetHashCode());
            Assert.Equal(versionInfo1, versionInfo2);
            Assert.True(versionInfo1.Equals(versionInfo2));
            Assert.True(versionInfo1.Equals((object)versionInfo2));
            Assert.True(versionInfo2.Equals(versionInfo1));
            Assert.True(versionInfo2.Equals((object)versionInfo1));
        }

        [Fact]
        public void Equals_returns_false_if_argument_is_not_a_VersionInfo()
        {
            var version = NuGetVersion.Parse("1.2.3-alpha");
            var commit = new GitId("123abc");
            var sut = new VersionInfo(version, commit);

            Assert.False(sut.Equals(new object()));
        }
    }
}
