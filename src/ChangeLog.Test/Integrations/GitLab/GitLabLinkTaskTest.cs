using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GitLabApiClient.Internal.Paths;
using GitLabApiClient.Models.Issues.Responses;
using GitLabApiClient.Models.MergeRequests.Requests;
using GitLabApiClient.Models.MergeRequests.Responses;
using GitLabApiClient.Models.Milestones.Requests;
using GitLabApiClient.Models.Milestones.Responses;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Integrations.GitLab;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Model.Text;
using Grynwald.ChangeLog.Pipeline;
using Grynwald.ChangeLog.Tasks;
using Grynwald.ChangeLog.Test.Configuration;
using Grynwald.ChangeLog.Test.Git;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Grynwald.ChangeLog.Test.Integrations.GitLab
{
    /// <summary>
    /// Unit tests for <see cref="GitLabLinkTask"/>
    /// </summary>
    public class GitLabLinkTaskTest : TestBase
    {
        public class GitLabProjectInfoTestCase : IXunitSerializable
        {
            public string Description { get; private set; }

            public IReadOnlyList<GitRemote> Remotes { get; set; } = Array.Empty<GitRemote>();

            public ChangeLogConfiguration.GitLabIntegrationConfiguration Configuration { get; set; } = new ChangeLogConfiguration.GitLabIntegrationConfiguration();

            public string ExpectedHost { get; set; } = "";

            public string ExpectedNamespace { get; set; } = "";

            public string ExpectedProject { get; set; } = "";


            public GitLabProjectInfoTestCase(string description)
            {
                if (String.IsNullOrWhiteSpace(description))
                    throw new ArgumentException("Value must not be null or whitespace", nameof(description));

                Description = description;

            }

            [Obsolete("For use by Xunit only", true)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
            public GitLabProjectInfoTestCase()
            { }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


            public void Deserialize(IXunitSerializationInfo info)
            {
                Description = info.GetValue<string>(nameof(Description));
                Remotes = info.GetValue<XunitSerializableGitRemote[]>(nameof(Remotes)).Select(x => x.Value).ToArray();
                Configuration = info.GetValue<XunitSerializableGitLabIntegrationConfiguration>(nameof(Configuration));
                ExpectedHost = info.GetValue<string>(nameof(ExpectedHost));
                ExpectedNamespace = info.GetValue<string>(nameof(ExpectedNamespace));
                ExpectedProject = info.GetValue<string>(nameof(ExpectedProject));
            }

            public void Serialize(IXunitSerializationInfo info)
            {
                info.AddValue(nameof(Description), Description);
                info.AddValue(nameof(Remotes), Remotes.Select(x => new XunitSerializableGitRemote(x)).ToArray());
                info.AddValue(nameof(Configuration), new XunitSerializableGitLabIntegrationConfiguration(Configuration));
                info.AddValue(nameof(ExpectedHost), ExpectedHost);
                info.AddValue(nameof(ExpectedNamespace), ExpectedNamespace);
                info.AddValue(nameof(ExpectedProject), ExpectedProject);
            }

            public override string ToString() => Description;
        }

        private readonly ILogger<GitLabLinkTask> m_Logger = NullLogger<GitLabLinkTask>.Instance;
        private readonly ChangeLogConfiguration m_DefaultConfiguration = ChangeLogConfigurationLoader.GetDefaultConfiguration();
        private readonly Mock<IGitLabClientFactory> m_ClientFactoryMock;
        private readonly GitLabClientMock m_ClientMock;
        private readonly Mock<IGitRepository> m_RepositoryMock;

        public GitLabLinkTaskTest()
        {
            m_ClientMock = new();
            m_ClientFactoryMock = new(MockBehavior.Strict);
            m_ClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(m_ClientMock.Object);
            m_RepositoryMock = new Mock<IGitRepository>(MockBehavior.Strict);
        }

        private ProjectId MatchProjectId(string expected)
        {
            return It.Is((ProjectId actual) => actual.ToString() == ((ProjectId)expected).ToString());
        }

        /// <summary>
        /// Helper method to test if an Action performs the expected changes to an object
        /// </summary>
        /// <typeparam name="T">The type of object the changes are applied to.</typeparam>
        /// <param name="actionToVerify">The action to apply to the object.</param>
        /// <param name="assertions">The assertions to apply to the object after applying <paramref name="actionToVerify"/></param>
        private bool AssertAction<T>(Action<T> actionToVerify, params Action<T>[] assertions)
        {
            // I'm not sure this is a good idea
            // but the only way we can make any assertions about an action
            // is calling it an afterwards checking if it made the expected
            // changes to the instance.
            // The instance in turn needs to be created through reflection because
            // GitLabApiClient's query types are sealed with an internal constructor

            var type = typeof(T);
            var instance = (T)type.Assembly.CreateInstance(
                type.FullName!, false,
                BindingFlags.Instance | BindingFlags.NonPublic,
                null, null, null, null)!;

            if (instance == null)
                throw new InvalidOperationException($"Failed to create instance of '{type.FullName}'");

            // Apply the action we want to verify
            actionToVerify.Invoke(instance);

            // Apply the assertions to the instance
            foreach (var assertion in assertions)
            {
                assertion.Invoke(instance);
            }

            return true;
        }


        [Fact]
        public async Task Run_does_nothing_if_repository_does_not_have_remotes()
        {
            // ARRANGE            
            m_RepositoryMock.SetupEmptyRemotes();

            var sut = new GitLabLinkTask(m_Logger, m_DefaultConfiguration, m_RepositoryMock.Object, m_ClientFactoryMock.Object);
            var changeLog = new ApplicationChangeLog();

            // ACT 
            var result = await sut.RunAsync(changeLog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Skipped, result);

            m_ClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData("not-a-url")]
        [InlineData("http://not-a-gitlab-url.com")]
        public async Task Run_does_nothing_if_remote_url_cannot_be_parsed(string url)
        {
            // ARRANGE            
            m_RepositoryMock.SetupRemotes("origin", url);

            var sut = new GitLabLinkTask(m_Logger, m_DefaultConfiguration, m_RepositoryMock.Object, m_ClientFactoryMock.Object);
            var changeLog = new ApplicationChangeLog();

            // ACT 
            var result = await sut.RunAsync(changeLog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Skipped, result);

            m_ClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData("gitlab.com")]
        [InlineData("example.com")]
        public async Task Run_creates_client_through_client_factory(string hostName)
        {
            // ARRANGE          
            m_RepositoryMock.SetupRemotes("origin", $"http://{hostName}/owner/repo.git");

            var sut = new GitLabLinkTask(m_Logger, m_DefaultConfiguration, m_RepositoryMock.Object, m_ClientFactoryMock.Object);

            // ACT 
            var result = await sut.RunAsync(new ApplicationChangeLog());

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);

            m_ClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Once);
            m_ClientFactoryMock.Verify(x => x.CreateClient(hostName), Times.Once);
        }

        [Theory]
        [InlineData("#23", 23, "owner/repo")]
        [InlineData("anotherRepo#42", 42, "owner/anotherRepo")]
        [InlineData("anotherOwner/anotherRepo#42", 42, "anotherOwner/anotherRepo")]
        public async Task Run_adds_issue_links_to_footers(string footerText, int id, string projectPath)
        {
            // ARRANGE            
            m_RepositoryMock.SetupRemotes("origin", "http://gitlab.com/owner/repo.git");

            m_ClientMock.Commits
                .SetupGetAsync()
                .ReturnsTestCommit(sha => $"https://example.com/{sha}");

            m_ClientMock.Issues
                .Setup(x => x.GetAsync(MatchProjectId(projectPath), id))
                .ReturnsAsync(
                    new Issue() { WebUrl = $"https://example.com/{projectPath}/issues/{id}" }
                );

            var sut = new GitLabLinkTask(m_Logger, m_DefaultConfiguration, m_RepositoryMock.Object, m_ClientFactoryMock.Object);

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
                    var expectedUri = new Uri($"https://example.com/{projectPath}/issues/{id}");

                    var referenceTextElement = Assert.IsType<GitLabReferenceTextElement>(footer.Value);
                    Assert.Equal(expectedUri, referenceTextElement.Uri);
                    Assert.Equal(footerText, referenceTextElement.Text);
                    Assert.Equal(GitLabReferenceType.Issue, referenceTextElement.Reference.Type);
                    Assert.Equal(projectPath, referenceTextElement.Reference.Project.ProjectPath);
                    Assert.Equal(id, referenceTextElement.Reference.Id);
                });

            });

            m_ClientMock.Issues.Verify(x => x.GetAsync(It.IsAny<ProjectId>(), It.IsAny<int>()), Times.Once);
            m_ClientMock.Issues.Verify(x => x.GetAsync(MatchProjectId(projectPath), id), Times.Once);
        }


        [Theory]
        [InlineData("#23", 23, "owner/repo")]
        [InlineData("anotherRepo#42", 42, "owner/anotherRepo")]
        [InlineData("anotherOwner/anotherRepo#42", 42, "anotherOwner/anotherRepo")]
        public async Task Run_does_not_add_link_if_issue_cannot_be_found(string footerText, int id, string projectPath)
        {
            // ARRANGE            
            m_RepositoryMock.SetupRemotes("origin", "http://gitlab.com/owner/repo.git");

            m_ClientMock.Commits
                .SetupGetAsync()
                .ReturnsTestCommit(sha => $"https://example.com/{sha}");

            m_ClientMock.Issues
                .SetupGetAsync()
                .ThrowsNotFound();

            var sut = new GitLabLinkTask(m_Logger, m_DefaultConfiguration, m_RepositoryMock.Object, m_ClientFactoryMock.Object);

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
                    Assert.False(footer.Value is IWebLinkTextElement, "Footer value should not contain a link");
                });

            });

            m_ClientMock.Issues.Verify(x => x.GetAsync(It.IsAny<ProjectId>(), It.IsAny<int>()), Times.Once);
            m_ClientMock.Issues.Verify(x => x.GetAsync(MatchProjectId(projectPath), id), Times.Once);
        }


        [Theory]
        [InlineData("!23", 23, "owner/repo")]
        [InlineData("anotherRepo!42", 42, "owner/anotherRepo")]
        [InlineData("anotherOwner/anotherRepo!42", 42, "anotherOwner/anotherRepo")]
        public async Task Run_adds_merge_request_links_to_footers(string footerText, int id, string projectPath)
        {
            // ARRANGE            
            m_RepositoryMock.SetupRemotes("origin", "http://gitlab.com/owner/repo.git");

            m_ClientMock.Commits
                .SetupGetAsync()
                .ReturnsTestCommit(sha => $"https://example.com/{sha}");

            m_ClientMock.MergeRequests
                .Setup(x => x.GetAsync(MatchProjectId(projectPath), It.IsAny<Action<ProjectMergeRequestsQueryOptions>>()))
                .ReturnsAsync(
                    new List<MergeRequest>() { new MergeRequest() { WebUrl = $"https://example.com/{projectPath}/merge_requests/{id}" } }
                );

            var sut = new GitLabLinkTask(m_Logger, m_DefaultConfiguration, m_RepositoryMock.Object, m_ClientFactoryMock.Object);

            var changeLog = new ApplicationChangeLog()
            {
                GetSingleVersionChangeLog(
                    "1.2.3",
                    null,
                    GetChangeLogEntry(summary: "Entry1", commit: TestGitIds.Id1, footers: new []
                    {
                        new ChangeLogEntryFooter(new CommitMessageFooterName("Merge-Request"), new PlainTextElement(footerText))
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
                Assert.All(entry.Footers.Where(x => x.Name == new CommitMessageFooterName("Merge-Request")), footer =>
                {
                    var expectedUri = new Uri($"https://example.com/{projectPath}/merge_requests/{id}");

                    var referenceTextElement = Assert.IsType<GitLabReferenceTextElement>(footer.Value);
                    Assert.Equal(expectedUri, referenceTextElement.Uri);
                    Assert.Equal(footerText, referenceTextElement.Text);
                    Assert.Equal(GitLabReferenceType.MergeRequest, referenceTextElement.Reference.Type);
                    Assert.Equal(projectPath, referenceTextElement.Reference.Project.ProjectPath);
                    Assert.Equal(id, referenceTextElement.Reference.Id);
                });

            });

            m_ClientMock.MergeRequests.Verify(
                x => x.GetAsync(It.IsAny<ProjectId>(), It.IsAny<Action<ProjectMergeRequestsQueryOptions>>()),
                Times.Once);

            m_ClientMock.MergeRequests.Verify(
                x => x.GetAsync(
                    MatchProjectId(projectPath),
                    It.Is<Action<ProjectMergeRequestsQueryOptions>>(
                        action =>
                            AssertAction(
                                action,
                                opts => Assert.Equal(id, Assert.Single(opts.MergeRequestsIds)),
                                opts => Assert.Equal(QueryMergeRequestState.All, opts.State)
                            ))),
                Times.Once);
        }

        [Theory]
        [InlineData("!23", "owner/repo")]
        [InlineData("anotherRepo!42", "owner/anotherRepo")]
        [InlineData("anotherOwner/anotherRepo!42", "anotherOwner/anotherRepo")]
        public async Task Run_does_not_add_link_if_merge_request_cannot_be_found(string footerText, string projectPath)
        {
            // ARRANGE            
            m_RepositoryMock.SetupRemotes("origin", "http://gitlab.com/owner/repo.git");

            m_ClientMock.Commits
                .SetupGetAsync()
                .ReturnsTestCommit(sha => $"https://example.com/{sha}");

            m_ClientMock.MergeRequests
                .Setup(x => x.GetAsync(It.IsAny<ProjectId>(), It.IsAny<Action<ProjectMergeRequestsQueryOptions>>()))
                .ThrowsNotFound();

            var sut = new GitLabLinkTask(m_Logger, m_DefaultConfiguration, m_RepositoryMock.Object, m_ClientFactoryMock.Object);

            var changeLog = new ApplicationChangeLog()
            {
                GetSingleVersionChangeLog(
                    "1.2.3",
                    null,
                    GetChangeLogEntry(summary: "Entry1", commit: TestGitIds.Id1, footers: new []
                    {
                        new ChangeLogEntryFooter(new CommitMessageFooterName("Merge-Request"), new PlainTextElement(footerText))
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
                Assert.All(entry.Footers.Where(x => x.Name == new CommitMessageFooterName("Merge-Request")), footer =>
                {
                    Assert.False(footer.Value is IWebLinkTextElement, "Footer value should not contain a link");
                });

            });

            m_ClientMock.MergeRequests.Verify(x => x.GetAsync(It.IsAny<ProjectId>(), It.IsAny<Action<ProjectMergeRequestsQueryOptions>>()), Times.Once);
            m_ClientMock.MergeRequests.Verify(x => x.GetAsync(MatchProjectId(projectPath), It.IsAny<Action<ProjectMergeRequestsQueryOptions>>()), Times.Once);
        }


        [Theory]
        [InlineData("%23", 23, "owner/repo")]
        [InlineData("anotherRepo%42", 42, "owner/anotherRepo")]
        [InlineData("anotherOwner/anotherRepo%42", 42, "anotherOwner/anotherRepo")]
        public async Task Run_adds_milestone_links_to_footers(string footerText, int id, string projectPath)
        {
            // ARRANGE            
            m_RepositoryMock.SetupRemotes("origin", "http://gitlab.com/owner/repo.git");

            m_ClientMock.Commits
                .SetupGetAsync()
                .ReturnsTestCommit(sha => $"https://example.com/{sha}");

            m_ClientMock.Projects
                .Setup(x => x.GetMilestonesAsync(MatchProjectId(projectPath), It.IsAny<Action<MilestonesQueryOptions>>()))
                .ReturnsAsync(
                    new List<Milestone>() { new() { WebUrl = $"https://example.com/{projectPath}/milestones/{id}" } }
                );

            var sut = new GitLabLinkTask(m_Logger, m_DefaultConfiguration, m_RepositoryMock.Object, m_ClientFactoryMock.Object);

            var changeLog = new ApplicationChangeLog()
            {
                GetSingleVersionChangeLog(
                    "1.2.3",
                    null,
                    GetChangeLogEntry(summary: "Entry1", commit: TestGitIds.Id1, footers: new []
                    {
                        new ChangeLogEntryFooter(new CommitMessageFooterName("Milestone"), new PlainTextElement(footerText))
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
                Assert.All(entry.Footers.Where(x => x.Name == new CommitMessageFooterName("Milestone")), footer =>
                {
                    var expectedUri = new Uri($"https://example.com/{projectPath}/milestones/{id}");

                    var referenceTextElement = Assert.IsType<GitLabReferenceTextElement>(footer.Value);
                    Assert.Equal(expectedUri, referenceTextElement.Uri);
                    Assert.Equal(footerText, referenceTextElement.Text);
                    Assert.Equal(GitLabReferenceType.Milestone, referenceTextElement.Reference.Type);
                    Assert.Equal(projectPath, referenceTextElement.Reference.Project.ProjectPath);
                    Assert.Equal(id, referenceTextElement.Reference.Id);
                });

            });

            m_ClientMock.Projects.Verify(
                x => x.GetMilestonesAsync(It.IsAny<ProjectId>(), It.IsAny<Action<MilestonesQueryOptions>>()),
                Times.Once);

            m_ClientMock.Projects.Verify(
                x => x.GetMilestonesAsync(
                    MatchProjectId(projectPath),
                    It.Is<Action<MilestonesQueryOptions>>(action =>
                        AssertAction(
                            action,
                            opts => Assert.Equal(id, Assert.Single(opts.MilestoneIds)),
                            opts => Assert.Equal(MilestoneState.All, opts.State)
                        ))),
                Times.Once);
        }

        [Theory]
        [InlineData("%23", "owner/repo")]
        [InlineData("anotherRepo%42", "owner/anotherRepo")]
        [InlineData("anotherOwner/anotherRepo%42", "anotherOwner/anotherRepo")]
        public async Task Run_does_not_add_link_if_milestone_cannot_be_found(string footerText, string projectPath)
        {
            // ARRANGE            
            m_RepositoryMock.SetupRemotes("origin", "http://gitlab.com/owner/repo.git");

            m_ClientMock.Commits
                .SetupGetAsync()
                .ReturnsTestCommit(sha => $"https://example.com/{sha}");

            m_ClientMock.Projects
                .SetupGetMilestonesAsync()
                .ThrowsNotFound();

            var sut = new GitLabLinkTask(m_Logger, m_DefaultConfiguration, m_RepositoryMock.Object, m_ClientFactoryMock.Object);

            var changeLog = new ApplicationChangeLog()
            {
                GetSingleVersionChangeLog(
                    "1.2.3",
                    null,
                    GetChangeLogEntry(summary: "Entry1", commit: TestGitIds.Id1, footers: new []
                    {
                        new ChangeLogEntryFooter(new CommitMessageFooterName("Milestone"), new PlainTextElement(footerText))
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
                Assert.All(entry.Footers.Where(x => x.Name == new CommitMessageFooterName("Milestone")), footer =>
                {
                    Assert.False(footer.Value is IWebLinkTextElement, "Footer value should not contain a link");
                });
            });

            m_ClientMock.Projects.Verify(x => x.GetMilestonesAsync(It.IsAny<ProjectId>(), It.IsAny<Action<MilestonesQueryOptions>>()), Times.Once);
            m_ClientMock.Projects.Verify(x => x.GetMilestonesAsync(MatchProjectId(projectPath), It.IsAny<Action<MilestonesQueryOptions>>()), Times.Once);
        }


        public static IEnumerable<object[]> GitLabProjectInfoTestCases()
        {
            yield return new[]
            {
                new GitLabProjectInfoTestCase("ProjectInfo from default remote (user namespace)")
                {
                    Remotes = new[]
                    {
                        new GitRemote("origin", "https://gitlab.com/someUser/someProject.git"),
                        new GitRemote("upstream", "https://example.com/upstreamUser/upstreamRepo.git"),
                        new GitRemote("some-other-remote", "https://example.com/someOtherUser/someOtherRepo.git")
                    },
                    Configuration = new ChangeLogConfiguration.GitLabIntegrationConfiguration()
                    {
                        RemoteName = "origin"
                    },
                    ExpectedHost = "gitlab.com",
                    ExpectedNamespace = "someUser",
                    ExpectedProject = "someProject"
                }
            };

            yield return new[]
            {
                new GitLabProjectInfoTestCase("ProjectInfo from default remote (group/subgroup namespace)")
                {
                    Remotes = new[]
                    {
                        new GitRemote("origin", "https://gitlab.com/group/subGroup/someProject.git"),
                        new GitRemote("upstream", "https://example.com/upstreamUser/upstreamRepo.git"),
                        new GitRemote("some-other-remote", "https://example.com/someOtherUser/someOtherRepo.git")
                    },
                    Configuration = new ChangeLogConfiguration.GitLabIntegrationConfiguration()
                    {
                        RemoteName = "origin"
                    },
                    ExpectedHost = "gitlab.com",
                    ExpectedNamespace = "group/subGroup",
                    ExpectedProject = "someProject"
                }
            };

            yield return new[]
            {
                new GitLabProjectInfoTestCase("ProjectInfo from custom remote name")
                {
                    Remotes = new[]
                    {
                        new GitRemote("origin", "https://gitlab.com/someUser/someRepo.git"),
                        new GitRemote("upstream", "https://example.com/upstreamUser/upstreamRepo.git"),
                        new GitRemote("some-other-remote", "https://example.com/someOtherProject/someOtherRepo.git"),
                    },
                    Configuration = new ChangeLogConfiguration.GitLabIntegrationConfiguration()
                    {
                        RemoteName = "upstream"
                    },
                    ExpectedHost = "example.com",
                    ExpectedNamespace = "upstreamUser",
                    ExpectedProject = "upstreamRepo"
                }
            };

            yield return new[]
            {
                new GitLabProjectInfoTestCase("ProjectInfo from configuration with no remotes")
                {
                    Remotes = Array.Empty<GitRemote>(),
                    Configuration = new ChangeLogConfiguration.GitLabIntegrationConfiguration()
                    {
                        Host = "example.com",
                        Namespace = "group/subgroup",
                        Project = "configRepo"
                    },
                    ExpectedHost = "example.com",
                    ExpectedNamespace = "group/subgroup",
                    ExpectedProject = "configRepo"
                }
            };

            yield return new[]
            {
                new GitLabProjectInfoTestCase("ProjectInfo from configuration with remotes")
                {
                    Remotes = new[]
                    {
                        new GitRemote("origin", "https://gitlab.com/remoteUrlNamespace/remoteUrlProject.git")
                    },
                    Configuration = new ChangeLogConfiguration.GitLabIntegrationConfiguration()
                    {
                        RemoteName = "origin",
                        Host = "example.com",
                        Namespace = "configNamespace",
                        Project = "configProject"
                    },
                    ExpectedHost = "example.com",
                    ExpectedNamespace = "configNamespace",
                    ExpectedProject = "configProject"
                }
            };

            yield return new[]
            {
                new GitLabProjectInfoTestCase("Host from config, namespace and project from remote url")
                {
                    Remotes = new[]
                    {
                        new GitRemote("origin", "https://gitlab.com/group/subgroup/remoteUrlProject.git")
                    },
                    Configuration = new ChangeLogConfiguration.GitLabIntegrationConfiguration()
                    {
                        RemoteName = "origin",
                        Host = "example.com",
                    },
                    ExpectedHost = "example.com",
                    ExpectedNamespace = "group/subgroup",
                    ExpectedProject = "remoteUrlProject"
                }
            };

            yield return new[]
            {
                new GitLabProjectInfoTestCase("Namespace from config, host and project from remote url")
                {
                    Remotes = new[]
                    {
                        new GitRemote("origin", "https://gitlab.com/remoteUrlNamespace/remoteUrlProject.git")
                    },
                    Configuration = new ChangeLogConfiguration.GitLabIntegrationConfiguration()
                    {
                        RemoteName = "origin",
                        Namespace = "configNamespace"
                    },
                    ExpectedHost = "gitlab.com",
                    ExpectedNamespace = "configNamespace",
                    ExpectedProject = "remoteUrlProject"
                }
            };

            yield return new[]
            {
                new GitLabProjectInfoTestCase("Project from config, namespace and host from remote url")
                {
                    Remotes = new[]
                    {
                        new GitRemote("origin", "https://gitlab.com/remoteUrlNamespace/remoteUrlRepo.git")
                    },
                    Configuration = new ChangeLogConfiguration.GitLabIntegrationConfiguration()
                    {
                        RemoteName = "origin",
                        Project = "configProject"
                    },
                    ExpectedHost = "gitlab.com",
                    ExpectedNamespace = "remoteUrlNamespace",
                    ExpectedProject = "configProject"
                }
            };

            yield return new[]
            {
                new GitLabProjectInfoTestCase("Host and namespace from config, project from remote url")
                {
                    Remotes = new[]
                    {
                        new GitRemote("origin", "https://gitlab.com/remoteUrlNamespace/remoteUrlProject.git")
                    },
                    Configuration = new ChangeLogConfiguration.GitLabIntegrationConfiguration()
                    {
                        RemoteName = "origin",
                        Host = "example.com",
                        Namespace = "configNamespace"
                    },
                    ExpectedHost = "example.com",
                    ExpectedNamespace = "configNamespace",
                    ExpectedProject = "remoteUrlProject"
                }
            };

            yield return new[]
            {
                new GitLabProjectInfoTestCase("Host and project from config, namespace from remote url")
                {
                    Remotes = new[]
                    {
                        new GitRemote("origin", "https://gitlab.com/remoteUrlNamespace/remoteUrlProject.git")
                    },
                    Configuration = new ChangeLogConfiguration.GitLabIntegrationConfiguration()
                    {
                        RemoteName = "origin",
                        Host = "example.com",
                        Project = "configProject"
                    },
                    ExpectedHost = "example.com",
                    ExpectedNamespace = "remoteUrlNamespace",
                    ExpectedProject = "configProject"
                }
            };

            yield return new[]
            {
                new GitLabProjectInfoTestCase("Project and namespace from config, host from remote url")
                {
                    Remotes = new[]
                    {
                        new GitRemote("origin", "https://gitlab.com/remoteUrlNamespace/remoteUrlProject.git")
                    },
                    Configuration = new ChangeLogConfiguration.GitLabIntegrationConfiguration()
                    {
                        RemoteName = "origin",
                        Namespace = "configNamespace",
                        Project = "configProject"
                    },
                    ExpectedHost = "gitlab.com",
                    ExpectedNamespace = "configNamespace",
                    ExpectedProject = "configProject"
                }
            };
        }

        [Theory]
        [MemberData(nameof(GitLabProjectInfoTestCases))]
        public async Task Run_uses_the_expected_remote_url(GitLabProjectInfoTestCase testCase)
        {
            //
            // ARRANGE
            //
            // Prepare changelog
            m_RepositoryMock.SetupRemotes(testCase.Remotes);

            m_ClientMock.Commits
                .SetupGetAsync()
                .ReturnsTestCommit(sha => "http://example.com");

            var changeLog = new ApplicationChangeLog()
            {
                GetSingleVersionChangeLog(
                    version: "1.2.3",
                    commitId: TestGitIds.Id1,
                    entries: new []
                    {
                        GetChangeLogEntry(
                            commit: TestGitIds.Id1,
                            footers: new[]
                            {
                                new ChangeLogEntryFooter(
                                    new("Commit"),
                                    new CommitReferenceTextElement(TestGitIds.Id1.ToString(), TestGitIds.Id1))
                            })
                    })
            };
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Integrations.GitLab = testCase.Configuration;

            var sut = new GitLabLinkTask(m_Logger, config, m_RepositoryMock.Object, m_ClientFactoryMock.Object);

            //
            // ACT
            //
            var result = await sut.RunAsync(changeLog);

            //
            // ASSERT
            //
            Assert.Equal(ChangeLogTaskResult.Success, result);

            m_ClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Once);
            m_ClientFactoryMock.Verify(x => x.CreateClient(testCase.ExpectedHost), Times.Once);

            m_ClientMock.Commits.Verify(x => x.GetAsync(It.IsAny<ProjectId>(), It.IsAny<string>()), Times.Once);
            m_ClientMock.Commits.Verify(x => x.GetAsync(MatchProjectId($"{testCase.ExpectedNamespace}/{testCase.ExpectedProject}"), TestGitIds.Id1.Id), Times.Once);
        }

        [Fact]
        public async Task Run_adds_web_links_to_commit_references()
        {
            // ARRANGE
            m_RepositoryMock.SetupRemotes("origin", "http://gitlab.com/user/repo.git");

            m_ClientMock.Commits
                .Setup(x => x.GetAsync(MatchProjectId("user/repo"), It.IsAny<string>()))
                .ReturnsTestCommit(sha => $"https://example.com/commit/{sha}");

            var sut = new GitLabLinkTask(m_Logger, m_DefaultConfiguration, m_RepositoryMock.Object, m_ClientFactoryMock.Object);

            var changeLog = new ApplicationChangeLog()
            {
                GetSingleVersionChangeLog(
                    "1.2.3",
                    null,
                    GetChangeLogEntry(summary: "Entry1", commit: TestGitIds.Id1, footers: new ChangeLogEntryFooter[]
                    {
                        new (new ("Footer1"), new CommitReferenceTextElement("id1", TestGitIds.Id1)),
                        new (new ("Footer1"), new CommitReferenceTextElement("id2", TestGitIds.Id2))
                    })
                )
            };

            // ACT
            var result = await sut.RunAsync(changeLog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);
            var entry = Assert.Single(changeLog.ChangeLogs.SelectMany(x => x.AllEntries));
            Assert.Collection(entry.Footers,
                x =>
                {
                    var expectedUri = new Uri($"https://example.com/commit/{TestGitIds.Id1.Id}");
                    var webLink = Assert.IsType<CommitReferenceTextElementWithWebLink>(x.Value);
                    Assert.Equal(expectedUri, webLink.Uri);
                },
                x =>
                {
                    var expectedUri = new Uri($"https://example.com/commit/{TestGitIds.Id2.Id}");
                    var webLink = Assert.IsType<CommitReferenceTextElementWithWebLink>(x.Value);
                    Assert.Equal(expectedUri, webLink.Uri);
                });
        }

        [Fact]
        public async Task Run_ignores_commit_references_that_cannot_be_found()
        {
            // ARRANGE
            m_RepositoryMock.SetupRemotes("origin", "http://gitlab.com/user/repo.git");

            m_ClientMock.Commits
                .Setup(x => x.GetAsync(MatchProjectId("user/repo"), It.IsAny<string>()))
                .ThrowsNotFound();

            var sut = new GitLabLinkTask(m_Logger, m_DefaultConfiguration, m_RepositoryMock.Object, m_ClientFactoryMock.Object);

            var changeLog = new ApplicationChangeLog()
            {
                GetSingleVersionChangeLog(
                    "1.2.3",
                    null,
                    GetChangeLogEntry(summary: "Entry1", commit: TestGitIds.Id1, footers: new ChangeLogEntryFooter[]
                    {
                        new (new ("Footer1"), new CommitReferenceTextElement("id1", TestGitIds.Id1)),
                    })
                )
            };

            // ACT
            var result = await sut.RunAsync(changeLog);

            // ASSERT
            Assert.Equal(ChangeLogTaskResult.Success, result);
            var entry = Assert.Single(changeLog.ChangeLogs.SelectMany(x => x.AllEntries));
            Assert.Collection(entry.Footers, x => Assert.IsType<CommitReferenceTextElement>(x.Value));
        }
    }
}
