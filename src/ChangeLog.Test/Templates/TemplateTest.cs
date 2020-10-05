using System;
using System.Collections.Generic;
using System.IO;
using ApprovalTests;
using ApprovalTests.Reporters;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Templates;
using Grynwald.ChangeLog.Test.Tasks;
using Grynwald.Utilities.IO;
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
        public void ChangeLog_is_converted_to_expected_Markdown_01()
        {
            // Empty changelog
            Approve(new ApplicationChangeLog());
        }

        [Fact]
        public void ChangeLog_is_converted_to_expected_Markdown_02()
        {
            // Empty changelog
            var changelog = new ApplicationChangeLog()
            {
                GetSingleVersionChangeLog("1.2.3")
            };

            Approve(changelog);
        }

        [Fact]
        public void ChangeLog_is_converted_to_expected_Markdown_03()
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
        public void ChangeLog_is_converted_to_expected_Markdown_04()
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
        public void ChangeLog_is_converted_to_expected_Markdown_05()
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
        public void ChangeLog_is_converted_to_expected_Markdown_06()
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
        public void ChangeLog_is_converted_to_expected_Markdown_07()
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
        public void ChangeLog_is_converted_to_expected_Markdown_08()
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
        public void ChangeLog_is_converted_to_expected_Markdown_09()
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
        public void ChangeLog_is_converted_to_expected_Markdown_10()
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
        public void ChangeLog_is_converted_to_expected_Markdown_11()
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
        public void ChangeLog_is_converted_to_expected_Markdown_12()
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
        public void ChangeLog_is_converted_to_expected_Markdown_13()
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
        public void ChangeLog_is_converted_to_expected_Markdown_14()
        {
            // Footers must be included in the output

            var versionChangeLog = GetSingleVersionChangeLog(
                "1.2.3",
                null,

                GetChangeLogEntry(scope: "scope1", type: "feat", summary: "Some change", footers: new[]
                {
                    new ChangeLogEntryFooter(new CommitMessageFooterName("See-Also"), "Issue #5")
                }),

                GetChangeLogEntry(scope: "scope2", type: "fix", summary: "A bug was fixed", footers: new[]
                {
                    new ChangeLogEntryFooter(new CommitMessageFooterName("Reviewed-by"), "someone@example.com")
                })
            );

            var changeLog = new ApplicationChangeLog() { versionChangeLog };

            Approve(changeLog);
        }

        [Fact]
        public void ChangeLog_is_converted_to_expected_Markdown_15()
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
                    new ChangeLogEntryFooter(new CommitMessageFooterName("See-Also"), "Issue #5")
                }),

                GetChangeLogEntry(scope: "scope2", type: "fix", summary: "A bug was fixed", footers: new[]
                {
                    new ChangeLogEntryFooter(new CommitMessageFooterName("Reviewed-by"), "someone@example.com")
                })
            );

            var changeLog = new ApplicationChangeLog() { versionChangeLog };

            Approve(changeLog, config);
        }

        [Fact]
        public void ChangeLog_is_converted_to_expected_Markdown_16()
        {
            // if an entry's CommitWebUri is set, the link must be included in the output

            var entry = GetChangeLogEntry(scope: "scope1", type: "feat", summary: "Some change");
            entry.CommitWebUri = new Uri("http://example.com/some-link");

            var versionChangeLog = GetSingleVersionChangeLog("1.2.3", null, entry);

            var changeLog = new ApplicationChangeLog() { versionChangeLog };

            Approve(changeLog);
        }

        [Fact]
        public void ChangeLog_is_converted_to_expected_Markdown_17()
        {
            // if an footer's WebUri is set, the link must be included in the output

            var entry = GetChangeLogEntry(scope: "scope1", type: "feat", summary: "Some change", footers: new[]
            {
                new ChangeLogEntryFooter(new CommitMessageFooterName("see-also"), "Link") { WebUri = new Uri("http://example.com") }
            });

            var versionChangeLog = GetSingleVersionChangeLog("1.2.3", null, entry);

            var changeLog = new ApplicationChangeLog() { versionChangeLog };

            Approve(changeLog);
        }

        [Fact]
        public void ChangeLog_is_converted_to_expected_Markdown_18()
        {
            // all configured types are included in the output, in the configured order
            var config = ChangeLogConfigurationLoader.GetDefaultConfiguration();
            config.EntryTypes = new Dictionary<string, ChangeLogConfiguration.EntryTypeConfiguration>()
            {
                { "feat", new ChangeLogConfiguration.EntryTypeConfiguration() { DisplayName = "New Features" } },
                { "docs", new ChangeLogConfiguration.EntryTypeConfiguration() { DisplayName = "Documentation Updates" } },
                { "fix", new ChangeLogConfiguration.EntryTypeConfiguration() { DisplayName = "Fixed bugs" } }
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
    }
}
