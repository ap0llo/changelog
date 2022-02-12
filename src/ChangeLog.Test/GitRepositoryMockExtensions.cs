using System;
using System.Collections.Generic;
using System.Linq;
using Grynwald.ChangeLog.Git;
using Moq;
using Moq.Language.Flow;

namespace Grynwald.ChangeLog.Test
{
    /// <summary>
    /// Extension methods to ease mocking of <see cref="IGitRepository"/>
    /// </summary>
    internal static class GitRepositoryMockExtensions
    {
        public static IReturnsResult<IGitRepository> SetupRemotes(this Mock<IGitRepository> mock, string name, string url)
        {
            return mock.SetupRemotes(new GitRemote(name, url));
        }

        public static IReturnsResult<IGitRepository> SetupRemotes(this Mock<IGitRepository> mock, params GitRemote[] remotes)
        {
            return mock.Setup(x => x.Remotes).Returns(remotes);
        }

        public static IReturnsResult<IGitRepository> SetupRemotes(this Mock<IGitRepository> mock, IEnumerable<GitRemote> remotes)
        {
            return mock.Setup(x => x.Remotes).Returns(remotes);
        }

        public static IReturnsResult<IGitRepository> SetupEmptyRemotes(this Mock<IGitRepository> mock)
        {
            return mock.Setup(x => x.Remotes).Returns(Enumerable.Empty<GitRemote>());
        }

        public static IReturnsResult<IGitRepository> SetupTryGetCommit(this Mock<IGitRepository> mock)
        {
            return mock.Setup(x => x.TryGetCommit(It.IsAny<string>())).Returns((GitCommit?)null);
        }

        public static IReturnsResult<IGitRepository> SetupTryGetCommit(this Mock<IGitRepository> mock, GitCommit commit)
        {
            return mock.Setup(x => x.TryGetCommit(It.Is<string>(str => commit.Id.Id.StartsWith(str, StringComparison.OrdinalIgnoreCase)))).Returns(commit);
        }
    }
}
