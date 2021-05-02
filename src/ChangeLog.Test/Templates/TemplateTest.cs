using System;
using System.Collections.Generic;
using System.IO;
using ApprovalTests;
using ApprovalTests.Reporters;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Model.Text;
using Grynwald.ChangeLog.Templates;
using Grynwald.ChangeLog.Test.Tasks;
using Grynwald.Utilities.IO;
using Moq;
using Xunit;

namespace Grynwald.ChangeLog.Test.Templates
{
    [UseReporter(typeof(DiffReporter))]
    public abstract class TemplateTest : TestBase
    {
        protected void Approve(ApplicationChangeLog changeLog, ChangeLogConfiguration? configuration = null)
        {
            var sut = GetTemplateInstance(configuration ?? ChangeLogConfigurationLoader.GetDefaultConfiguration());

            using (var temporaryDirectory = new TemporaryDirectory())
            {
                var outputPath = Path.Combine(temporaryDirectory, "changelog.md");
                sut.SaveChangeLog(changeLog, outputPath);

                Assert.True(File.Exists(outputPath));

                var output = File.ReadAllText(outputPath);

                var writer = new ApprovalTextWriter(output);
                Approvals.Verify(writer, new ApprovalNamer(GetType().Name), Approvals.GetReporter());
            }
        }

        protected abstract ITemplate GetTemplateInstance(ChangeLogConfiguration configuration);


        [Fact]
        public void ChangeLog_is_converted_to_expected_Output_01()
        {
            // Empty changelog
            Approve(new ApplicationChangeLog());
        }

        [Fact]
        public void ChangeLog_is_converted_to_expected_Output_02()
        {
            // Empty changelog
            var changelog = new ApplicationChangeLog()
            {
                GetSingleVersionChangeLog("1.2.3")
            };

            Approve(changelog);
        }

        [Fact]
        public void ChangeLog_is_converted_to_expected_Output_03()
        {
            // Changelog with a single versions and multiple entries

            var versionChangeLog = GetSingleVersionChangeLog(
                "1.2.3",
                null,
                GetChangeLogEntry(type: "feat", summary: "Some change"),
                GetChangeLogEntry(type: "fix", summary: "A bug was fixed"),
                GetChangeLogEntry(type: "feat", summary: "Some other change")
            );

            var changeLog = new ApplicationChangeLog() { versionChangeLog };

            Approve(changeLog);
        }

        [Fact]
        public void ChangeLog_is_converted_to_expected_Output_04()
        {
            // Changelog with a single versions and multiple entries (including entries with scope)

            var versionChangeLog = GetSingleVersionChangeLog(
                "1.2.3",
                null,
                GetChangeLogEntry(scope: "api", type: "feat", summary: "Some change"),
                GetChangeLogEntry(scope: "cli", type: "fix", summary: "A bug was fixed"),
                GetChangeLogEntry(scope: "", type: "feat", summary: "Some other change")
            );

            var changeLog = new ApplicationChangeLog() { versionChangeLog };

            Approve(changeLog);
        }

        [Fact]
        public void ChangeLog_is_converted_to_expected_Output_05()
        {
            // Changelog that contains entry with a body

            var versionChangeLog = GetSingleVersionChangeLog(
                "1.2.3",
                null,

                GetChangeLogEntry(
                    scope: "api",
                    type: "feat",
                    summary: "Some change",
                    body: new[]
                    {
                        "Changelog entry body Line1\r\nLine2",
                        "Changelog entry body Line3\r\nLine4",
                    }),

                GetChangeLogEntry(scope: "cli", type: "fix", summary: "A bug was fixed")
            );

            var changeLog = new ApplicationChangeLog() { versionChangeLog };

            Approve(changeLog);
        }

        [Fact]
        public void ChangeLog_is_converted_to_expected_Output_06()
        {
            // Changelog with only a single entry 
            // (Changelog uses simpler format if there is only a single entry for a version)

            var versionChangeLog = GetSingleVersionChangeLog(
                "1.2.3",
                null,

                GetChangeLogEntry(
                    scope: "api",
                    type: "feat",
                    summary: "Some change",
                    body: new[]
                    {
                        "Changelog entry body Line1\r\nLine2",
                        "Changelog entry body Line3\r\nLine4",
                    })
            );

            var changeLog = new ApplicationChangeLog() { versionChangeLog };

            Approve(changeLog);
        }

        [Fact]
        public void ChangeLog_is_converted_to_expected_Output_07()
        {
            // Changelog that includes breaking changes
            // Breaking changes must be included regardless of the change type

            var versionChangeLog = GetSingleVersionChangeLog(
                "1.2.3",
                null,

                GetChangeLogEntry(
                    scope: "api",
                    type: "feat",
                    summary: "Some change",
                    body: new[]
                    {
                        "Changelog entry body Line1\r\nLine2",
                        "Changelog entry body Line3\r\nLine4",
                    }),

                GetChangeLogEntry(scope: "cli", type: "fix", summary: "A bug was fixed"),

                GetChangeLogEntry(isBreakingChange: true, type: "refactor", summary: "Some breaking change"),

                GetChangeLogEntry(isBreakingChange: true, type: "fix", summary: "A breaking bugfix")
            );


            var changeLog = new ApplicationChangeLog() { versionChangeLog };

            Approve(changeLog);
        }

        [Fact]
        public void ChangeLog_is_converted_to_expected_Output_08()
        {
            // Changelog uses simpler format if there is only a single entry for a version
            // When a entry is a feature AND a breaking change, it must not count as two entries

            var versionChangeLog = GetSingleVersionChangeLog(
                "1.2.3",
                null,

                GetChangeLogEntry(
                    scope: "api",
                    type: "feat",
                    isBreakingChange: true,
                    summary: "Some change",
                    body: new[]
                    {
                        "Changelog entry body Line1\r\nLine2",
                        "Changelog entry body Line3\r\nLine4",
                    })
            );

            var changeLog = new ApplicationChangeLog() { versionChangeLog };

            Approve(changeLog);
        }

        [Fact]
        public void ChangeLog_is_converted_to_expected_Output_09()
        {
            // Breaking changes must be included regardless of the change type

            var versionChangeLog = GetSingleVersionChangeLog(
                "1.2.3",
                null,

                GetChangeLogEntry(
                    scope: "api",
                    type: "feat",
                    summary: "Some change",
                    body: new[]
                    {
                        "Changelog entry body Line1\r\nLine2",
                        "Changelog entry body Line3\r\nLine4",
                    }),

                GetChangeLogEntry(scope: "cli", type: "fix", summary: "A bug was fixed"),

                GetChangeLogEntry(isBreakingChange: true, type: "refactor", summary: "Some breaking change"),

                GetChangeLogEntry(
                    type: "fix",
                    summary: "A breaking bugfix",
                    breakingChangeDescriptions: new[]
                    {
                        "Description of breaking change"
                    })
            );

            var changeLog = new ApplicationChangeLog() { versionChangeLog };

            Approve(changeLog);
        }

        [Fact]
        public void ChangeLog_is_converted_to_expected_Output_10()
        {
            // Changelog uses simpler format if there is only a single entry for a version
            // When a entry is a feature AND a breaking change, it must not count as two entries

            var versionChangeLog = GetSingleVersionChangeLog(
                "1.2.3",
                null,

                GetChangeLogEntry(
                    scope: "api",
                    type: "feat",
                    summary: "Some change",
                    body: new[]
                    {
                        "Changelog entry body Line1\r\nLine2",
                        "Changelog entry body Line3\r\nLine4",
                    },
                    breakingChangeDescriptions: new[]
                    {
                        "Description of breaking change"
                    })
            );

            var changeLog = new ApplicationChangeLog() { versionChangeLog };

            Approve(changeLog);
        }

        [Fact]
        public void ChangeLog_is_converted_to_expected_Output_11()
        {
            // Breaking changes must be included regardless of the change type

            var versionChangeLog = GetSingleVersionChangeLog(
                "1.2.3",
                null,

                GetChangeLogEntry(
                    scope: "api",
                    type: "feat",
                    summary: "Some change",
                    body: new[]
                    {
                        "Changelog entry body Line1\r\nLine2",
                        "Changelog entry body Line3\r\nLine4",
                    }),

                GetChangeLogEntry(scope: "cli", type: "fix", summary: "A bug was fixed"),

                GetChangeLogEntry(isBreakingChange: true, type: "refactor", summary: "Some breaking change"),

                GetChangeLogEntry(
                    type: "fix",
                    summary: "A breaking bugfix",
                    breakingChangeDescriptions: new[]
                    {
                        "Description of breaking change",
                        "Another breaking change"
                    })
            );

            var changeLog = new ApplicationChangeLog() { versionChangeLog };

            Approve(changeLog);
        }

        [Fact]
        public void ChangeLog_is_converted_to_expected_Output_12()
        {
            // Changelog uses simpler format if there is only a single entry for a version
            // When a entry is a feature AND a breaking change, it must not count as two entries

            var versionChangeLog = GetSingleVersionChangeLog(
                "1.2.3",
                null,

                GetChangeLogEntry(
                    scope: "api",
                    type: "feat",
                    summary: "Some change",
                    body: new[]
                    {
                        "Changelog entry body Line1\r\nLine2",
                        "Changelog entry body Line3\r\nLine4",
                    },
                    breakingChangeDescriptions: new[]
                    {
                        "Description of breaking change",
                        "Another breaking change"
                    }));

            var changeLog = new ApplicationChangeLog() { versionChangeLog };

            Approve(changeLog);
        }

        [Fact]
        public void ChangeLog_is_converted_to_expected_Output_13()
        {
            // if a display name is configured for a scope,
            // it must be used in the output instead of the actual scope

            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Scopes = new Dictionary<string, ChangeLogConfiguration.ScopeConfiguration>()
            {
                { "scope1", new ChangeLogConfiguration.ScopeConfiguration() { DisplayName = "Scope 1 Display Name" } }
            };


            var versionChangeLog = GetSingleVersionChangeLog(
                "1.2.3",
                null,

                GetChangeLogEntry(scope: "scope1", type: "feat", summary: "Some change"),

                GetChangeLogEntry(scope: "scope2", type: "fix", summary: "A bug was fixed")
            );

            var changeLog = new ApplicationChangeLog() { versionChangeLog };

            Approve(changeLog, config);
        }

        [Fact]
        public void ChangeLog_is_converted_to_expected_Output_14()
        {
            // Footers must be included in the output

            var versionChangeLog = GetSingleVersionChangeLog(
                "1.2.3",
                null,

                GetChangeLogEntry(scope: "scope1", type: "feat", summary: "Some change", footers: new[]
                {
                    new ChangeLogEntryFooter(new CommitMessageFooterName("See-Also"), new PlainTextElement("Issue #5"))
                }),

                GetChangeLogEntry(scope: "scope2", type: "fix", summary: "A bug was fixed", footers: new[]
                {
                    new ChangeLogEntryFooter(new CommitMessageFooterName("Reviewed-by"), new PlainTextElement("someone@example.com"))
                })
            );

            var changeLog = new ApplicationChangeLog() { versionChangeLog };

            Approve(changeLog);
        }

        [Fact]
        public void ChangeLog_is_converted_to_expected_Output_15()
        {
            // if a display name is configured for a footer,
            // the output must use the display name instead of the footer name

            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.Footers = new Dictionary<string, ChangeLogConfiguration.FooterConfiguration>
            {
                { "see-also", new ChangeLogConfiguration.FooterConfiguration() { DisplayName = "See Also" } },
                {  "reviewed-by", new ChangeLogConfiguration.FooterConfiguration() { DisplayName = "Reviewed by" } }
            };

            var versionChangeLog = GetSingleVersionChangeLog(
                "1.2.3",
                null,

                GetChangeLogEntry(scope: "scope1", type: "feat", summary: "Some change", footers: new[]
                {
                    new ChangeLogEntryFooter(new CommitMessageFooterName("See-Also"), new PlainTextElement("Issue #5"))
                }),

                GetChangeLogEntry(scope: "scope2", type: "fix", summary: "A bug was fixed", footers: new[]
                {
                    new ChangeLogEntryFooter(new CommitMessageFooterName("Reviewed-by"), new PlainTextElement("someone@example.com"))
                })
            );

            var changeLog = new ApplicationChangeLog() { versionChangeLog };

            Approve(changeLog, config);
        }

        [Fact]
        public void ChangeLog_is_converted_to_expected_Output_16()
        {
            // if an entry's "Commit" footer contains a web link, it must be included in the output
            var entry = GetChangeLogEntry(
                scope: "scope1",
                type: "feat",
                summary: "Some change",
                commit: TestGitIds.Id1,
                footers: new[]
                {
                    new ChangeLogEntryFooter(
                        new("Commit"),
                        new CommitReferenceTextElementWithWebLink(
                            TestGitIds.Id1.ToString(),
                            TestGitIds.Id1,
                            new("http://example.com/some-link")))
                });

            var versionChangeLog = GetSingleVersionChangeLog("1.2.3", null, entry);

            var changeLog = new ApplicationChangeLog() { versionChangeLog };

            Approve(changeLog);
        }

        [Fact]
        public void ChangeLog_is_converted_to_expected_Output_17()
        {
            // if an footer's WebUri is set, the link must be included in the output

            var entry = GetChangeLogEntry(scope: "scope1", type: "feat", summary: "Some change", footers: new[]
            {
                new ChangeLogEntryFooter(
                    new CommitMessageFooterName("see-also"),
                    new WebLinkTextElement("Link", new Uri("http://example.com"))
                )
            });

            var versionChangeLog = GetSingleVersionChangeLog("1.2.3", null, entry);

            var changeLog = new ApplicationChangeLog() { versionChangeLog };

            Approve(changeLog);
        }

        [Fact]
        public void ChangeLog_is_converted_to_expected_Output_18()
        {
            // all configured types are included in the output, in the configured order
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.EntryTypes = new Dictionary<string, ChangeLogConfiguration.EntryTypeConfiguration>()
            {
                { "feat", new ChangeLogConfiguration.EntryTypeConfiguration() { DisplayName = "New Features", Priority = 100 } },
                { "docs", new ChangeLogConfiguration.EntryTypeConfiguration() { DisplayName = "Documentation Updates", Priority = 80 } },
                { "fix", new ChangeLogConfiguration.EntryTypeConfiguration() { DisplayName = "Fixed bugs", Priority = 90 } }
            };
            var versionChangeLog = GetSingleVersionChangeLog("1.2.3", entries: new[]
            {
                GetChangeLogEntry(type: "docs", summary: "A documentation change"),
                GetChangeLogEntry(type: "fix", summary: "Some bug fix"),
                GetChangeLogEntry(type: "feat", summary: "Some feature"),
            });

            var changeLog = new ApplicationChangeLog() { versionChangeLog };

            Approve(changeLog, config);
        }

        [Fact]
        public void ChangeLog_is_converted_to_expected_Output_19()
        {
            // Footers which's value is a ChangeLogEntryReferenceTextElement are rendered as links to the referenced entries

            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();

            var entry1 = GetChangeLogEntry(type: "fix", scope: "scope", summary: "Some bug fix", commit: TestGitIds.Id1);
            var entry2 = GetChangeLogEntry(
                    type: "feat",
                    summary: "Some feature",
                    commit: TestGitIds.Id2,
                    footers: new[]
                    {
                        new ChangeLogEntryFooter(
                            new CommitMessageFooterName("See-Also"),
                            new ChangeLogEntryReferenceTextElement("irrelevant", entry1))
                    });

            var changeLog = new ApplicationChangeLog() { GetSingleVersionChangeLog("1.2.3", entries: new[] { entry1, entry2 }) };

            Approve(changeLog, config);
        }

        private class CustomTextElementWithLink : IWebLinkTextElement
        {
            public string Text => "Example";

            public TextStyle Style => TextStyle.None;

            public Uri Uri { get; } = new Uri("https://example.com");

            public CustomTextElementWithLink()
            { }
        }

        [Fact]
        public void ChangeLog_is_converted_to_expected_Output_20()
        {
            // Footers which's value is a instance of IWebLinkTextElement are rendered as links

            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();

            var entry1 = GetChangeLogEntry(type: "fix", summary: "Some bug fix", commit: TestGitIds.Id1);
            var entry2 = GetChangeLogEntry(
                    type: "feat",
                    summary: "Some feature",
                    commit: TestGitIds.Id2,
                    footers: new[]
                    {
                        new ChangeLogEntryFooter(
                            new CommitMessageFooterName("See-Also"),
                            new CustomTextElementWithLink())
                    });

            var changeLog = new ApplicationChangeLog() { GetSingleVersionChangeLog("1.2.3", entries: new[] { entry1, entry2 }) };

            Approve(changeLog, config);
        }

        [Fact]
        public void ChangeLog_is_converted_to_expected_Output_21()
        {
            // Footers which's value is a instance of CommitReferenceTextElement are rendered as code spans

            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();

            var entry1 = GetChangeLogEntry(type: "fix", summary: "Some bug fix", commit: TestGitIds.Id1);
            var entry2 = GetChangeLogEntry(
                    type: "feat",
                    summary: "Some feature",
                    commit: TestGitIds.Id2,
                    footers: new[]
                    {
                        new ChangeLogEntryFooter(
                            new CommitMessageFooterName("See-Also"),
                            new CommitReferenceTextElement(TestGitIds.Id1.ToString(), TestGitIds.Id1))
                    });

            var changeLog = new ApplicationChangeLog() { GetSingleVersionChangeLog("1.2.3", entries: new[] { entry1, entry2 }) };

            Approve(changeLog, config);
        }
    }
}
