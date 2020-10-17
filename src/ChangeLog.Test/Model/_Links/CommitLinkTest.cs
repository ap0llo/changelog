using System;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Model;
using Xunit;

namespace Grynwald.ChangeLog.Test.Model
{
    /// <summary>
    /// Tests for <see cref="CommitLink"/>
    /// </summary>
    public class CommitLinkTest
    {
        [Fact]
        public void Commit_id_must_not_be_null()
        {
            // ARRANGE
            var id = default(GitId);

            // ACT 
            var ex = Record.Exception(() => new CommitLink(id));

            // ASSERT
            Assert.NotNull(ex);
            var argumentException = Assert.IsType<ArgumentException>(ex);
            Assert.Equal("id", argumentException.ParamName);
        }
    }
}
