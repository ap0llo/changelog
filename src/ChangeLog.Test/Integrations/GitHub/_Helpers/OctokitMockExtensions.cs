using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Moq;
using Moq.Language.Flow;
using Octokit;

namespace Grynwald.ChangeLog.Test.Integrations.GitHub
{
    /// <summary>
    /// Extension methods to ease mocking of Oktokit types
    /// </summary>
    internal static class OctokitMockExtensions
    {
        public static ISetup<IRepositoryCommitsClient, Task<GitHubCommit>> SetupGet(this Mock<IRepositoryCommitsClient> mock)
        {
            return mock.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
        }

        public static ISetup<IIssuesClient, Task<Issue>> SetupGet(this Mock<IIssuesClient> mock)
        {
            return mock.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));
        }

        public static ISetup<IPullRequestsClient, Task<PullRequest>> SetupGet(this Mock<IPullRequestsClient> mock)
        {
            return mock.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));
        }


        public static IReturnsResult<IRepositoryCommitsClient> ReturnsTestCommit(this ISetup<IRepositoryCommitsClient, Task<GitHubCommit>> setup)
        {
            return setup.ReturnsAsync((string _, string _, string sha) => TestGitHubCommit.FromCommitSha(sha));
        }

        public static IReturnsResult<IIssuesClient> ReturnsTestIssue(this ISetup<IIssuesClient, Task<Issue>> setup)
        {
            return setup.ReturnsAsync((string _, string _, int issueNumber) => TestGitHubIssue.FromIssueNumber(issueNumber));
        }

        public static IReturnsResult<IPullRequestsClient> ReturnsTestPullRequest(this ISetup<IPullRequestsClient, Task<PullRequest>> setup)
        {
            return setup.ReturnsAsync((string _, string _, int prNumber) => TestGitHubPullRequest.FromPullRequestNumber(prNumber));
        }


        public static IReturnsResult<IRepositoryCommitsClient> ThrowsNotFound(this ISetup<IRepositoryCommitsClient, Task<GitHubCommit>> setup)
        {
            return setup.ThrowsAsync(new NotFoundException("Not found", HttpStatusCode.NotFound));
        }

        public static IReturnsResult<IIssuesClient> ThrowsNotFound(this ISetup<IIssuesClient, Task<Issue>> setup)
        {
            return setup.ThrowsAsync(new NotFoundException("Not found", HttpStatusCode.NotFound));
        }

        public static IReturnsResult<IPullRequestsClient> ThrowsNotFound(this ISetup<IPullRequestsClient, Task<PullRequest>> setup)
        {
            return setup.ThrowsAsync(new NotFoundException("Not found", HttpStatusCode.NotFound));
        }

        public static ISetup<IRepositoryContentsClient, Task<IReadOnlyList<RepositoryContent>>> SetupGetAllContents(this Mock<IRepositoryContentsClient> mock)
        {
            return mock.Setup(x => x.GetAllContents(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
        }

        public static IReturnsResult<IRepositoryContentsClient> ThrowsNotFound(this ISetup<IRepositoryContentsClient, Task<IReadOnlyList<RepositoryContent>>> setup)
        {
            return setup.ThrowsAsync(new NotFoundException("Not found", HttpStatusCode.NotFound));
        }

        public static IReturnsResult<IRepositoryContentsClient> ReturnsTestRepositoryContent(
            this ISetup<IRepositoryContentsClient, Task<IReadOnlyList<RepositoryContent>>> setup,
            params TestRepositoryContent[] files)
        {
            return setup.ReturnsAsync((string _, string _, string _) => files);
        }

        public static IReturnsResult<IRepositoryContentsClient> ReturnsTestRepositoryContent(
          this ISetup<IRepositoryContentsClient, Task<IReadOnlyList<RepositoryContent>>> setup,
          IEnumerable<TestRepositoryContent> files)
        {
            return setup.ReturnsAsync((string _, string _, string _) => files.ToList());
        }
    }
}
