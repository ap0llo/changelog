using System;
using System.Collections.Generic;
using Grynwald.ChangeLog.Git;
using Xunit;

namespace Grynwald.ChangeLog.Test.Git
{
    /// <summary>
    /// Tests for <see cref="GitNote"/>
    /// </summary>
    public class GitNoteTest : EqualityTest<GitNote, GitNoteTest>, IEqualityTestDataProvider<GitNote>
    {
        public IEnumerable<(GitNote left, GitNote right)> GetEqualTestCases()
        {
            yield return (new GitNote(TestGitIds.Id1, "commits", "message"), new GitNote(TestGitIds.Id1, "commits", "message"));
        }

        public IEnumerable<(GitNote left, GitNote right)> GetUnequalTestCases()
        {
            yield return (
                new GitNote(TestGitIds.Id1, "commits", "message"),
                new GitNote(TestGitIds.Id2, "commits", "message")
            );

            yield return (
                new GitNote(TestGitIds.Id1, "commits", "message"),
                new GitNote(TestGitIds.Id1, "custom", "message")
            );

            yield return (
                new GitNote(TestGitIds.Id1, "commits", "message"),
                new GitNote(TestGitIds.Id1, "commits", "some-other-message")
            );
        }


        [Fact]
        public void Target_must_not_be_null()
        {
            // ARRANGE
            var target = default(GitId);

            // ACT 
            var ex = Record.Exception(() => new GitNote(target: target, "commits", "message"));

            // ASSERT
            Assert.NotNull(ex);
            var argumentException = Assert.IsType<ArgumentException>(ex);
            Assert.Equal("target", argumentException.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        public void Namespace_must_not_be_null_or_whitespace(string @namespace)
        {
            // ARRANGE

            // ACT 
            var ex = Record.Exception(() => new GitNote(TestGitIds.Id1, @namespace: @namespace, "message"));

            // ASSERT
            var argumentException = Assert.IsType<ArgumentException>(ex);
            Assert.Equal("namespace", argumentException.ParamName);
        }

        [Fact]
        public void Message_must_not_be_null()
        {
            // ARRANGE

            // ACT 
            var ex = Record.Exception(() => new GitNote(TestGitIds.Id1, "commits", message: null!));

            // ASSERT
            Assert.NotNull(ex);
            var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
            Assert.Equal("message", argumentNullException.ParamName);
        }
    }
}
