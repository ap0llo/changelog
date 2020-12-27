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
    }
}
