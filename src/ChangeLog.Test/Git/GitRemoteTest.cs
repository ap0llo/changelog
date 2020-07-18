using System;
using System.Collections.Generic;
using Grynwald.ChangeLog.Git;
using Xunit;

namespace Grynwald.ChangeLog.Test.Git
{
    /// <summary>
    /// Tests for <see cref="GitRemote"/>
    /// </summary>
    public class GitRemoteTest : EqualityTest<GitRemote, GitRemoteTest>, IEqualityTestDataProvider<GitRemote>
    {
        public IEnumerable<(GitRemote left, GitRemote right)> GetEqualTestCases()
        {
            yield return (new GitRemote("origin", "http://example.com"), new GitRemote("origin", "http://example.com"));
            yield return (new GitRemote("upstream", "http://example.com/upstream"), new GitRemote("upstream", "http://example.com/upstream"));
        }

        public IEnumerable<(GitRemote left, GitRemote right)> GetUnequalTestCases()
        {
            yield return (new GitRemote("origin", "http://example.com"), new GitRemote("upstream", "http://example.com"));
            yield return (new GitRemote("origin", "http://example.com"), new GitRemote("origin", "http://example.net"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("\t")]
        public void Name_must_not_be_null_or_whitespace(string name)
        {
            Assert.Throws<ArgumentException>(() => new GitRemote(name, "http://example.com"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("\t")]
        public void Url_must_not_be_null_or_whitespace(string url)
        {
            Assert.Throws<ArgumentException>(() => new GitRemote("origin", url));
        }
    }
}
