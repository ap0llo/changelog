using Moq;
using Octokit;

namespace Grynwald.ChangeLog.Test.Integrations.GitHub
{
    internal class GitHubClientMock
    {
        public class RepositoriesClientMock
        {
            private readonly Mock<IRepositoriesClient> m_Mock = new(MockBehavior.Strict);

            public IRepositoriesClient Object => m_Mock.Object;

            public Mock<IRepositoryCommitsClient> Commit { get; } = new(MockBehavior.Strict);


            public RepositoriesClientMock()
            {
                m_Mock.Setup(x => x.Commit).Returns(Commit.Object);

            }
        }

        private readonly Mock<IGitHubClient> m_Mock = new(MockBehavior.Strict);


        public IGitHubClient Object => m_Mock.Object;

        public Mock<IIssuesClient> Issue { get; } = new(MockBehavior.Strict);

        public Mock<IPullRequestsClient> PullRequest { get; } = new(MockBehavior.Strict);

        public Mock<IMiscellaneousClient> Miscellaneous { get; } = new(MockBehavior.Strict);

        public RepositoriesClientMock Repository { get; } = new();


        public GitHubClientMock()
        {
            Miscellaneous
                .Setup(x => x.GetRateLimits())
                .ReturnsAsync(new MiscellaneousRateLimit(new ResourceRateLimit(), new RateLimit()));

            m_Mock.Setup(x => x.Repository).Returns(Repository.Object);
            m_Mock.Setup(x => x.Issue).Returns(Issue.Object);
            m_Mock.Setup(x => x.PullRequest).Returns(PullRequest.Object);
            m_Mock.Setup(x => x.Miscellaneous).Returns(Miscellaneous.Object);
        }
    }
}
