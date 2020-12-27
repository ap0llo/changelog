using GitLabApiClient;
using Moq;

namespace Grynwald.ChangeLog.Test.Integrations.GitLab
{
    internal class GitLabClientMock
    {
        private readonly Mock<IGitLabClient> m_Mock = new(MockBehavior.Strict);

        public IGitLabClient Object => m_Mock.Object;

        public Mock<ICommitsClient> Commits { get; } = new(MockBehavior.Strict);

        public Mock<IIssuesClient> Issues { get; } = new(MockBehavior.Strict);

        public Mock<IMergeRequestsClient> MergeRequests { get; } = new(MockBehavior.Strict);

        public Mock<IProjectsClient> Projects { get; } = new(MockBehavior.Strict);


        public GitLabClientMock()
        {
            m_Mock.Setup(x => x.Commits).Returns(Commits.Object);
            m_Mock.Setup(x => x.Issues).Returns(Issues.Object);
            m_Mock.Setup(x => x.MergeRequests).Returns(MergeRequests.Object);
            m_Mock.Setup(x => x.Projects).Returns(Projects.Object);
        }
    }
}
