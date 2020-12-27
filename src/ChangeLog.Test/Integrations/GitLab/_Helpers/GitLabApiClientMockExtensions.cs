using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using GitLabApiClient;
using GitLabApiClient.Internal.Paths;
using GitLabApiClient.Models.Commits.Responses;
using GitLabApiClient.Models.Issues.Responses;
using GitLabApiClient.Models.MergeRequests.Responses;
using GitLabApiClient.Models.Milestones.Requests;
using GitLabApiClient.Models.Milestones.Responses;
using Moq;
using Moq.Language.Flow;

namespace Grynwald.ChangeLog.Test.Integrations.GitLab
{
    /// <summary>
    /// Extension methods to ease mocking of GitLabApiClient types
    /// </summary>
    internal static class GitLabApiClientMockExtensions
    {
        public static ISetup<ICommitsClient, Task<Commit>> SetupGetAsync(this Mock<ICommitsClient> mock)
        {
            return mock.Setup(x => x.GetAsync(It.IsAny<ProjectId>(), It.IsAny<string>()));
        }

        public static ISetup<IIssuesClient, Task<Issue>> SetupGetAsync(this Mock<IIssuesClient> mock)
        {
            return mock.Setup(x => x.GetAsync(It.IsAny<ProjectId>(), It.IsAny<int>()));
        }

        public static ISetup<IMergeRequestsClient, Task<MergeRequest>> SetupGetAsync(this Mock<IMergeRequestsClient> mock)
        {
            return mock.Setup(x => x.GetAsync(It.IsAny<ProjectId>(), It.IsAny<int>()));
        }

        public static ISetup<IProjectsClient, Task<IList<Milestone>>> SetupGetMilestonesAsync(this Mock<IProjectsClient> mock)
        {
            return mock.Setup(x => x.GetMilestonesAsync(It.IsAny<ProjectId>(), It.IsAny<Action<MilestonesQueryOptions>>()));
        }


        public static IReturnsResult<ICommitsClient> ReturnsTestCommit(this ISetup<ICommitsClient, Task<Commit>> setup, Func<string, string> shaToWebUrl)
        {
            return setup.ReturnsAsync(
                (ProjectId id, string sha) => new Commit() { Id = sha, WebUrl = shaToWebUrl(sha) }
            );
        }


        public static IReturnsResult<ICommitsClient> ThrowsNotFound(this ISetup<ICommitsClient, Task<Commit>> setup)
        {
            return setup.ThrowsAsync(new GitLabException(HttpStatusCode.NotFound));
        }

        public static IReturnsResult<IIssuesClient> ThrowsNotFound(this ISetup<IIssuesClient, Task<Issue>> setup)
        {
            return setup.ThrowsAsync(new GitLabException(HttpStatusCode.NotFound));
        }

        public static IReturnsResult<IMergeRequestsClient> ThrowsNotFound(this ISetup<IMergeRequestsClient, Task<MergeRequest>> setup)
        {
            return setup.ThrowsAsync(new GitLabException(HttpStatusCode.NotFound));
        }

        public static IReturnsResult<IMergeRequestsClient> ThrowsNotFound(this ISetup<IMergeRequestsClient, Task<IList<MergeRequest>>> setup)
        {
            return setup.ThrowsAsync(new GitLabException(HttpStatusCode.NotFound));
        }

        public static IReturnsResult<IProjectsClient> ThrowsNotFound(this ISetup<IProjectsClient, Task<IList<Milestone>>> setup)
        {
            return setup.ThrowsAsync(new GitLabException(HttpStatusCode.NotFound));
        }
    }
}
