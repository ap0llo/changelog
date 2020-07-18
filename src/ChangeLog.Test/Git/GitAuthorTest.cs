using System;
using System.Collections.Generic;
using Grynwald.ChangeLog.Git;
using Xunit;

namespace Grynwald.ChangeLog.Test.Git
{
    /// <summary>
    /// Tests for <see cref="GitAuthor"/>
    /// </summary>
    public class GitAuthorTest : EqualityTest<GitAuthor, GitAuthorTest>, IEqualityTestDataProvider<GitAuthor>
    {
        public IEnumerable<(GitAuthor left, GitAuthor right)> GetEqualTestCases()
        {
            yield return (new GitAuthor("user", "user@example.com"), new GitAuthor("user", "user@example.com"));
        }

        public IEnumerable<(GitAuthor left, GitAuthor right)> GetUnequalTestCases()
        {
            yield return (new GitAuthor("user1", "user@example.com"), new GitAuthor("user2", "user@example.com"));
            yield return (new GitAuthor("user", "user1@example.com"), new GitAuthor("user", "user2@example.com"));
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("\t")]
        public void Name_must_not_be_null_or_whitespace(string name)
        {
            Assert.Throws<ArgumentException>(() => new GitAuthor(name, "user@example.com"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("\t")]
        public void Email_must_not_be_null_or_whitespace(string email)
        {
            Assert.Throws<ArgumentException>(() => new GitAuthor("user", email));
        }

    }
}
