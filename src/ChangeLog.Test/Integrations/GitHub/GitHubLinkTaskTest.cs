using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Integrations.GitHub;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Model.Text;
using Grynwald.ChangeLog.Tasks;
using Grynwald.ChangeLog.Test.Configuration;
using Grynwald.ChangeLog.Test.Git;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Octokit;
using Xunit;
using Xunit.Abstractions;

namespace Grynwald.ChangeLog.Test.Integrations.GitHub
{

    /// <summary>
    /// Unit tests for <see cref="GitHubLinkTask"/>
    /// </summary>
    public class GitHubLinkTaskTest : TestBase
    {
        public class GitHubProjectInfoTestCase : IXunitSerializable
        {
            public string Description { get; private set; }

            public IReadOnlyList<GitRemote> Remotes { get; set; } = Array.Empty<GitRemote>();

            public ChangeLogConfiguration.GitHubIntegrationConfiguration Configuration { get; set; } = new ChangeLogConfiguration.GitHubIntegrationConfiguration();

            public string ExpectedHost { get; set; } = "";

            public string ExpectedOwner { get; set; } = "";

            public string ExpectedRepository { get; set; } = "";


            public GitHubProjectInfoTestCase(string description)
            {
                if (String.IsNullOrWhiteSpace(description))
                    throw new ArgumentException("Value must not be null or whitespace", nameof(description));

                Description = description;

            }

            [Obsolete("For use by Xunit only", true)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
            public GitHubProjectInfoTestCase()
            { }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


            public void Deserialize(IXunitSerializationInfo info)
            {
                Description = info.GetValue<string>(nameof(Description));
                Remotes = info.GetValue<XunitSerializableGitRemote[]>(nameof(Remotes)).Select(x => x.Value).ToArray();
                Configuration = info.GetValue<XunitSerializableGitHubIntegrationConfiguration>(nameof(Configuration));
                ExpectedHost = info.GetValue<string>(nameof(ExpectedHost));
                ExpectedOwner = info.GetValue<string>(nameof(ExpectedOwner));
                ExpectedRepository = info.GetValue<string>(nameof(ExpectedRepository));
            }

            public void Serialize(IXunitSerializationInfo info)
            {
                info.AddValue(nameof(Description), Description);
                info.AddValue(nameof(Remotes), Remotes.Select(x => new XunitSerializableGitRemote(x)).ToArray());
                info.AddValue(nameof(Configuration), new XunitSerializableGitHubIntegrationConfiguration(Configuration));
                info.AddValue(nameof(ExpectedHost), ExpectedHost);
                info.AddValue(nameof(ExpectedOwner), ExpectedOwner);
                info.AddValue(nameof(ExpectedRepository), ExpectedRepository);
            }

            public override string ToString() => Description;
        }

        private readonly ILogger<GitHubLinkTask> m_Logger = NullLogger<GitHubLinkTask>.Instance;
        private readonly ChangeLogConfiguration m_DefaultConfiguration = ChangeLogConfigurationLoader.GetDefaultConfiguration();
        private readonly GitHubClientMock m_GithubClientMock;
        private readonly Mock<IGitHubClientFactory> m_GitHubClientFactoryMock;

        public GitHubLinkTaskTest()
        {
            m_GithubClientMock = new();

            m_GitHubClientFactoryMock = new(MockBehavior.Strict);
            m_GitHubClientFactoryMock
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(m_GithubClientMock.Object);
        }


        [Fact]
        public void Logger_must_not_be_null()
        {
            // ARRANGE

            // ACT
            var ex = Record.Exception(() => new GitHubLinkTask(null!, m_DefaultConfiguration, Mock.Of<IGitRepository>(MockBehavior.Strict), Mock.Of<IGitHubClientFactory>(MockBehavior.Strict)));

            // ASSERT
            var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
            Assert.Equal("logger", argumentNullException.ParamName);
        }

        [Fact]
        public void Configuration_must_not_be_null()
        {
            // ARRANGE

            // ACT
            var ex = Record.Exception(() => new GitHubLinkTask(m_Logger, null!, Mock.Of<IGitRepository>(MockBehavior.Strict), Mock.Of<IGitHubClientFactory>(MockBehavior.Strict)));

            // ASSERT
            var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
            Assert.Equal("configuration", argumentNullException.ParamName);
        }

        [Fact]
        public void GitRepository_must_not_be_null()
        {
            // ARRANGE

            // ACT
            var ex = Record.Exception(() => new GitHubLinkTask(m_Logger, m_DefaultConfiguration, null!, Mock.Of<IGitHubClientFactory>(MockBehavior.Strict)));

            // ASSERT
            var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
            Assert.Equal("repository", argumentNullException.ParamName);
        }

        [Fact]
        public void GitHubClientFactoty_must_not_be_null()
        {
            // ARRANGE

            // ACT
            var ex = Record.Exception(() => new GitHubLinkTask(m_Logger, m_DefaultConfiguration, Mock.Of<IGitRepository>(MockBehavior.Strict), null!));

            // ASSERT
            var argumentNullException = Assert.IsType<ArgumentNullException>(ex);
            Assert.Equal("gitHubClientFactory", argumentNullException.ParamName);
        }

        [Fact]
        public async Task Run_does_nothing_if_repository_does_not_have_remotes()
        {
            // ARRANGE
            var repoMock = new Mock<IGitRepository>(MockBehavior.Strict);
            repoMock.SetupEmptyRemotes();

            var sut = new GitHubLinkTask(m_Logger, m_DefaultConfiguration, repoMock.Object, m_GitHubClientFactoryMock.Object);
            var changeLog = new ApplicationChangeLog();

            // ACT 
            var result = await sut.RunAsync(changeLog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Skipped, result);
            m_GitHubClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData("origin", "not-a-url")]
        [InlineData("origin", "http://not-a-github-url.com")]
        [InlineData("some-other-remote", "not-a-url")]
        [InlineData("some-other-remote", "http://not-a-github-url.com")]
        public async Task Run_does_nothing_if_remote_url_cannot_be_parsed(string remoteName, string url)
        {
            // ARRANGE
            var configuration = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            configuration.Integrations.GitHub.RemoteName = remoteName;

            var repoMock = new Mock<IGitRepository>(MockBehavior.Strict);
            repoMock.SetupRemotes(remoteName, url);

            var sut = new GitHubLinkTask(m_Logger, configuration, repoMock.Object, m_GitHubClientFactoryMock.Object);
            var changeLog = new ApplicationChangeLog();

            // ACT 
            var result = await sut.RunAsync(changeLog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Skipped, result);
            m_GitHubClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Never);
        }

        public static IEnumerable<object[]> GitHubProjectInfoTestCases()
        {
            yield return new[]
            {
                new GitHubProjectInfoTestCase("ProjectInfo from default remote")
                {
                    Remotes = new[]
                    {
                        new GitRemote("origin", "https://github.com/someUser/someRepo.git"),
                        new GitRemote("upstream", "https://example.com/upstreamUser/upstreamRepo.git"),
                        new GitRemote("some-other-remote", "https://example.com/someOtherOwner/someOtherRepo.git")
                    },
                    Configuration = new ChangeLogConfiguration.GitHubIntegrationConfiguration()
                    {
                        RemoteName = "origin"
                    },
                    ExpectedHost = "github.com",
                    ExpectedOwner = "someUser",
                    ExpectedRepository = "someRepo"
                }
            };

            yield return new[]
            {
                new GitHubProjectInfoTestCase("ProjectInfo from custom remote name")
                {
                    Remotes = new[]
                    {
                        new GitRemote("origin", "https://github.com/someUser/someRepo.git"),
                        new GitRemote("upstream", "https://example.com/upstreamUser/upstreamRepo.git"),
                        new GitRemote("some-other-remote", "https://example.com/someOtherOwner/someOtherRepo.git"),
                    },
                    Configuration = new ChangeLogConfiguration.GitHubIntegrationConfiguration()
                    {
                        RemoteName = "upstream"
                    },
                    ExpectedHost = "example.com",
                    ExpectedOwner = "upstreamUser",
                    ExpectedRepository = "upstreamRepo"
                }
            };

            yield return new[]
            {
                new GitHubProjectInfoTestCase("ProjectInfo from configuration with no remotes")
                {
                    Remotes = Array.Empty<GitRemote>(),
                    Configuration = new ChangeLogConfiguration.GitHubIntegrationConfiguration()
                    {
                        Host = "example.com",
                        Owner = "configOwner",
                        Repository = "configRepo"
                    },
                    ExpectedHost = "example.com",
                    ExpectedOwner = "configOwner",
                    ExpectedRepository = "configRepo"
                }
            };

            yield return new[]
            {
                new GitHubProjectInfoTestCase("ProjectInfo from configuration with remotes")
                {
                    Remotes = new[]
                    {
                        new GitRemote("origin", "https://github.com/remoteUrlOwner/remoteUrlRepo.git")
                    },
                    Configuration = new ChangeLogConfiguration.GitHubIntegrationConfiguration()
                    {
                        RemoteName = "origin",
                        Host = "example.com",
                        Owner = "configOwner",
                        Repository = "configRepo"
                    },
                    ExpectedHost = "example.com",
                    ExpectedOwner = "configOwner",
                    ExpectedRepository = "configRepo"
                }
            };

            yield return new[]
            {
                new GitHubProjectInfoTestCase("Host from config, owner and repository from remote url")
                {
                    Remotes = new[]
                    {
                        new GitRemote("origin", "https://github.com/remoteUrlOwner/remoteUrlRepo.git")
                    },
                    Configuration = new ChangeLogConfiguration.GitHubIntegrationConfiguration()
                    {
                        RemoteName = "origin",
                        Host = "example.com",
                    },
                    ExpectedHost = "example.com",
                    ExpectedOwner = "remoteUrlOwner",
                    ExpectedRepository = "remoteUrlRepo"
                }
            };

            yield return new[]
            {
                new GitHubProjectInfoTestCase("Owner from config, host and repository from remote url")
                {
                    Remotes = new[]
                    {
                        new GitRemote("origin", "https://github.com/remoteUrlOwner/remoteUrlRepo.git")
                    },
                    Configuration = new ChangeLogConfiguration.GitHubIntegrationConfiguration()
                    {
                        RemoteName = "origin",
                        Owner = "configOwner"
                    },
                    ExpectedHost = "github.com",
                    ExpectedOwner = "configOwner",
                    ExpectedRepository = "remoteUrlRepo"
                }
            };

            yield return new[]
            {
                new GitHubProjectInfoTestCase("Repository from config, owner and host from remote url")
                {
                    Remotes = new[]
                    {
                        new GitRemote("origin", "https://github.com/remoteUrlOwner/remoteUrlRepo.git")
                    },
                    Configuration = new ChangeLogConfiguration.GitHubIntegrationConfiguration()
                    {
                        RemoteName = "origin",
                        Repository = "configRepo"
                    },
                    ExpectedHost = "github.com",
                    ExpectedOwner = "remoteUrlOwner",
                    ExpectedRepository = "configRepo"
                }
            };

            yield return new[]
            {
                new GitHubProjectInfoTestCase("Host and owner from config, repository from remote url")
                {
                    Remotes = new[]
                    {
                        new GitRemote("origin", "https://github.com/remoteUrlOwner/remoteUrlRepo.git")
                    },
                    Configuration = new ChangeLogConfiguration.GitHubIntegrationConfiguration()
                    {
                        RemoteName = "origin",
                        Host = "example.com",
                        Owner = "configOwner"
                    },
                    ExpectedHost = "example.com",
                    ExpectedOwner = "configOwner",
                    ExpectedRepository = "remoteUrlRepo"
                }
            };

            yield return new[]
            {
                new GitHubProjectInfoTestCase("Host and repository from config, owner from remote url")
                {
                    Remotes = new[]
                    {
                        new GitRemote("origin", "https://github.com/remoteUrlOwner/remoteUrlRepo.git")
                    },
                    Configuration = new ChangeLogConfiguration.GitHubIntegrationConfiguration()
                    {
                        RemoteName = "origin",
                        Host = "example.com",
                        Repository = "configRepo"
                    },
                    ExpectedHost = "example.com",
                    ExpectedOwner = "remoteUrlOwner",
                    ExpectedRepository = "configRepo"
                }
            };

            yield return new[]
            {
                new GitHubProjectInfoTestCase("Repository and owner from config, host from remote url")
                {
                    Remotes = new[]
                    {
                        new GitRemote("origin", "https://github.com/remoteUrlOwner/remoteUrlRepo.git")
                    },
                    Configuration = new ChangeLogConfiguration.GitHubIntegrationConfiguration()
                    {
                        RemoteName = "origin",
                        Owner = "configOwner",
                        Repository = "configRepo"
                    },
                    ExpectedHost = "github.com",
                    ExpectedOwner = "configOwner",
                    ExpectedRepository = "configRepo"
                }
            };
        }

        [Theory]
        [MemberData(nameof(GitHubProjectInfoTestCases))]
        public async Task Run_uses_the_expected_remote_url(GitHubProjectInfoTestCase testCase)
        {
            //
            // ARRANGE
            //

            // Prepare GitHub client
            m_GithubClientMock.Repository.Commit
                .SetupGet()
                .ReturnsTestCommit();

            // Prepare Git Repository
            var repoMock = new Mock<IGitRepository>(MockBehavior.Strict);
            repoMock.SetupRemotes(testCase.Remotes);

            // Configure remote name to use
            var configuration = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            configuration.Integrations.GitHub = testCase.Configuration;

            // Prepare changelog
            var changeLog = new ApplicationChangeLog()
            {
                GetSingleVersionChangeLog(
                    version: "1.2.3",
                    commitId: TestGitIds.Id1,
                    entries: new []{ GetChangeLogEntry(commit: TestGitIds.Id1) })
            };

            var sut = new GitHubLinkTask(m_Logger, configuration, repoMock.Object, m_GitHubClientFactoryMock.Object);

            //
            // ACT 
            //
            var result = await sut.RunAsync(changeLog);

            //
            // ASSERT
            //

            Assert.Equal(ChangeLogTaskResult.Success, result);

            // Ensure the web link was requested from the expected server and repository
            m_GitHubClientFactoryMock.Verify(x => x.CreateClient(testCase.ExpectedHost), Times.Once);
            m_GithubClientMock.Repository.Commit.Verify(x => x.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            m_GithubClientMock.Repository.Commit.Verify(x => x.Get(testCase.ExpectedOwner, testCase.ExpectedRepository, TestGitIds.Id1.Id), Times.Once);
        }


        [Fact]
        public async Task Run_adds_a_link_to_all_commits_if_url_can_be_parsed()
        {
            // ARRANGE
            var repoMock = new Mock<IGitRepository>(MockBehavior.Strict);
            repoMock.SetupRemotes("origin", "http://github.com/owner/repo.git");

            m_GithubClientMock.Repository.Commit
                .Setup(x => x.Get("owner", "repo", It.IsAny<string>()))
                .ReturnsTestCommit();

            var sut = new GitHubLinkTask(m_Logger, m_DefaultConfiguration, repoMock.Object, m_GitHubClientFactoryMock.Object);

            var changeLog = new ApplicationChangeLog()
            {
                GetSingleVersionChangeLog(
                    "1.2.3",
                    null,
                    GetChangeLogEntry(summary: "Entry1", commit: TestGitIds.Id1),
                    GetChangeLogEntry(summary: "Entry2", commit: TestGitIds.Id2)
                ),
                GetSingleVersionChangeLog(
                    "4.5.6",
                    null,
                    GetChangeLogEntry(summary: "Entry1", commit: TestGitIds.Id3),
                    GetChangeLogEntry(summary: "Entry2", commit: TestGitIds.Id4)
                )
            };

            // ACT 
            var result = await sut.RunAsync(changeLog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);
            var entries = changeLog.ChangeLogs.SelectMany(x => x.AllEntries).ToArray();
            Assert.All(entries, entry =>
            {
                Assert.NotNull(entry.CommitWebUri);
                var expectedUri = TestGitHubCommit.FromCommitSha(entry.Commit.Id).HtmlUri;
                Assert.Equal(expectedUri, entry.CommitWebUri);

                m_GithubClientMock.Repository.Commit.Verify(x => x.Get("owner", "repo", entry.Commit.Id), Times.Once);
            });

            m_GithubClientMock.Repository.Commit.Verify(x => x.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(entries.Length));
        }

        //TODO: Run does not add a commit link if commit cannot be found

        [Theory]
        [InlineData("#23", 23, "owner", "repo")]
        [InlineData("GH-23", 23, "owner", "repo")]
        [InlineData("anotherOwner/anotherRepo#42", 42, "anotherOwner", "anotherRepo")]
        [InlineData("another-Owner/another-Repo#42", 42, "another-Owner", "another-Repo")]
        [InlineData("another.Owner/another.Repo#42", 42, "another.Owner", "another.Repo")]
        [InlineData("another_Owner/another_Repo#42", 42, "another_Owner", "another_Repo")]
        public async Task Run_adds_issue_links_to_footers(string footerText, int issueNumber, string owner, string repo)
        {
            // ARRANGE
            var repoMock = new Mock<IGitRepository>(MockBehavior.Strict);
            repoMock.SetupRemotes("origin", "http://github.com/owner/repo.git");

            m_GithubClientMock.Repository.Commit
                .SetupGet()
                .ReturnsTestCommit();

            m_GithubClientMock.Issue
                .Setup(x => x.Get(owner, repo, issueNumber))
                .ReturnsTestIssue();

            var sut = new GitHubLinkTask(m_Logger, m_DefaultConfiguration, repoMock.Object, m_GitHubClientFactoryMock.Object);

            var changeLog = new ApplicationChangeLog()
            {
                GetSingleVersionChangeLog(
                    "1.2.3",
                    null,
                    GetChangeLogEntry(summary: "Entry1", commit: TestGitIds.Id1, footers: new []
                    {
                        new ChangeLogEntryFooter(new CommitMessageFooterName("Issue"), new PlainTextElement(footerText))
                    })
                )
            };

            // ACT 
            var result = await sut.RunAsync(changeLog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);

            var entries = changeLog.ChangeLogs.SelectMany(x => x.AllEntries).ToArray();
            Assert.All(entries, entry =>
            {
                Assert.All(entry.Footers.Where(x => x.Name == new CommitMessageFooterName("Issue")), footer =>
                {
                    var expectedUri = TestGitHubIssue.FromIssueNumber(issueNumber).HtmlUri;

                    var webLink = Assert.IsType<WebLinkTextElement>(footer.Value);
                    Assert.Equal(expectedUri, webLink.Uri);
                    Assert.Equal(footerText, webLink.Text);
                });
            });

            m_GithubClientMock.Issue.Verify(x => x.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
        }

        [Theory]
        [InlineData("#23", 23, "owner", "repo")]
        [InlineData("GH-23", 23, "owner", "repo")]
        [InlineData("anotherOwner/anotherRepo#42", 42, "anotherOwner", "anotherRepo")]
        [InlineData("another-Owner/another-Repo#42", 42, "another-Owner", "another-Repo")]
        [InlineData("another.Owner/another.Repo#42", 42, "another.Owner", "another.Repo")]
        [InlineData("another_Owner/another_Repo#42", 42, "another_Owner", "another_Repo")]
        // Linking must ignore trailing and leading whitespace
        [InlineData(" #23", 23, "owner", "repo")]
        [InlineData("#23 ", 23, "owner", "repo")]
        [InlineData(" GH-23", 23, "owner", "repo")]
        [InlineData("GH-23 ", 23, "owner", "repo")]
        [InlineData(" anotherOwner/anotherRepo#42", 42, "anotherOwner", "anotherRepo")]
        [InlineData("anotherOwner/anotherRepo#42  ", 42, "anotherOwner", "anotherRepo")]
        public async Task Run_adds_pull_request_links_to_footers(string footerText, int prNumber, string owner, string repo)
        {
            // ARRANGE
            var repoMock = new Mock<IGitRepository>(MockBehavior.Strict);
            repoMock.SetupRemotes("origin", "http://github.com/owner/repo.git");

            m_GithubClientMock.Repository.Commit
                .SetupGet()
                .ReturnsTestCommit();

            m_GithubClientMock.Issue
                .Setup(x => x.Get(owner, repo, prNumber))
                .ThrowsNotFound();

            m_GithubClientMock.PullRequest
                .Setup(x => x.Get(owner, repo, prNumber))
                .ReturnsTestPullRequest();


            var sut = new GitHubLinkTask(m_Logger, m_DefaultConfiguration, repoMock.Object, m_GitHubClientFactoryMock.Object);

            var changeLog = new ApplicationChangeLog()
            {
                GetSingleVersionChangeLog(
                    "1.2.3",
                    null,
                    GetChangeLogEntry(summary: "Entry1", commit: TestGitIds.Id1, footers: new []
                    {
                        new ChangeLogEntryFooter(new CommitMessageFooterName("Issue"), new PlainTextElement(footerText))
                    })
                )
            };

            // ACT 
            var result = await sut.RunAsync(changeLog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);

            var entries = changeLog.ChangeLogs.SelectMany(x => x.AllEntries).ToArray();
            Assert.All(entries, entry =>
            {
                Assert.All(entry.Footers.Where(x => x.Name == new CommitMessageFooterName("Issue")), footer =>
                {
                    var expectedUri = TestGitHubPullRequest.FromPullRequestNumber(prNumber).HtmlUri;

                    var webLink = Assert.IsType<WebLinkTextElement>(footer.Value);
                    Assert.Equal(expectedUri, webLink.Uri);
                    Assert.Equal(footerText, webLink.Text);
                });

            });

            m_GithubClientMock.PullRequest.Verify(x => x.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            m_GithubClientMock.Issue.Verify(x => x.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Once);
        }

        [Theory]
        [InlineData("not-a-reference")]
        [InlineData("Not a/reference#0")]
        [InlineData("Not a/reference#xyz")]
        [InlineData("#xyz")]
        [InlineData("GH-xyz")]
        [InlineData("#1 2 3")]
        [InlineData("GH-1 2 3")]
        public async Task Run_ignores_footers_which_cannot_be_parsed(string footerText)
        {
            // ARRANGE
            var repoMock = new Mock<IGitRepository>(MockBehavior.Strict);
            repoMock.SetupRemotes("origin", "http://github.com/owner/repo.git");

            m_GithubClientMock.Repository.Commit
                .SetupGet()
                .ReturnsTestCommit();

            var sut = new GitHubLinkTask(m_Logger, m_DefaultConfiguration, repoMock.Object, m_GitHubClientFactoryMock.Object);

            var changeLog = new ApplicationChangeLog()
            {
                GetSingleVersionChangeLog(
                    "1.2.3",
                    null,
                    GetChangeLogEntry(summary: "Entry1", commit: TestGitIds.Id1, footers: new []
                    {
                        new ChangeLogEntryFooter(new CommitMessageFooterName("Irrelevant"), new PlainTextElement(footerText)),
                    })
                )
            };

            // ACT 
            var result = await sut.RunAsync(changeLog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);

            var entries = changeLog.ChangeLogs.SelectMany(x => x.AllEntries).ToArray();
            Assert.All(entries, entry =>
            {
                Assert.All(entry.Footers, footer =>
                {
                    Assert.False(footer.Value is IWebLinkTextElement, "Footer value should not contain a link");
                });

            });

            m_GithubClientMock.Issue.Verify(x => x.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
            m_GithubClientMock.PullRequest.Verify(x => x.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        [Theory]
        [InlineData("#23", "owner", "repo")]
        [InlineData("GH-23", "owner", "repo")]
        [InlineData("anotherOwner/anotherRepo#23", "anotherOwner", "anotherRepo")]
        public async Task Run_does_not_add_a_links_to_footers_if_no_issue_or_pull_request_cannot_be_found(string footerText, string owner, string repo)
        {
            // ARRANGE
            var repoMock = new Mock<IGitRepository>(MockBehavior.Strict);
            repoMock.SetupRemotes("origin", "http://github.com/owner/repo.git");

            m_GithubClientMock.Repository.Commit
                .SetupGet()
                .ReturnsTestCommit();

            m_GithubClientMock.Issue
                .SetupGet()
                .ThrowsNotFound();

            m_GithubClientMock.PullRequest
                .SetupGet()
                .ThrowsNotFound();

            var sut = new GitHubLinkTask(m_Logger, m_DefaultConfiguration, repoMock.Object, m_GitHubClientFactoryMock.Object);

            var changeLog = new ApplicationChangeLog()
            {
                GetSingleVersionChangeLog(
                    "1.2.3",
                    null,
                    GetChangeLogEntry(summary: "Entry1", commit: TestGitIds.Id1, footers: new []
                    {
                        new ChangeLogEntryFooter(new CommitMessageFooterName("Irrelevant"), new PlainTextElement(footerText)),
                    })
                )
            };

            // ACT 
            var result = await sut.RunAsync(changeLog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);

            var entries = changeLog.ChangeLogs.SelectMany(x => x.AllEntries).ToArray();
            Assert.All(entries, entry =>
            {
                Assert.All(entry.Footers, footer =>
                {
                    Assert.False(footer.Value is IWebLinkTextElement, "Footer value should not contain a link");
                });
            });

            m_GithubClientMock.Issue.Verify(x => x.Get(owner, repo, It.IsAny<int>()), Times.Once);
            m_GithubClientMock.PullRequest.Verify(x => x.Get(owner, repo, It.IsAny<int>()), Times.Once);
        }

        [Theory]
        [InlineData("github.com")]
        [InlineData("github.example.com")]
        [InlineData("some-domain.com")]
        public async Task Run_creates_client_through_client_factory(string hostName)
        {
            // ARRANGE
            var repoMock = new Mock<IGitRepository>(MockBehavior.Strict);
            repoMock.SetupRemotes("origin", $"http://{hostName}/owner/repo.git");

            var sut = new GitHubLinkTask(m_Logger, m_DefaultConfiguration, repoMock.Object, m_GitHubClientFactoryMock.Object);

            // ACT 
            var result = await sut.RunAsync(new ApplicationChangeLog());

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);

            m_GitHubClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Once);
            m_GitHubClientFactoryMock.Verify(x => x.CreateClient(hostName), Times.Once);
        }

        [Fact]
        public async Task Task_fails_if_GitHub_client_throws_an_ApiException()
        {
            // ARRANGE
            var repoMock = new Mock<IGitRepository>(MockBehavior.Strict);
            repoMock.SetupRemotes("origin", $"http://github.com/owner/repo.git");

            m_GithubClientMock.Repository.Commit
                .SetupGet()
                .ThrowsAsync(new ApiException());

            var sut = new GitHubLinkTask(m_Logger, m_DefaultConfiguration, repoMock.Object, m_GitHubClientFactoryMock.Object);

            var changeLog = new ApplicationChangeLog()
            {
                GetSingleVersionChangeLog(
                    "1.2.3",
                    null,
                    GetChangeLogEntry(summary: "Entry1", commit: TestGitIds.Id1)
                )
            };

            // ACT 
            var result = await sut.RunAsync(changeLog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Error, result);
        }
    }
}
