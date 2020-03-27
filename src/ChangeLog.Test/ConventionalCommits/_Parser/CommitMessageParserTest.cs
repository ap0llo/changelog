using System;
using System.Collections.Generic;
using Grynwald.ChangeLog.ConventionalCommits;
using Xunit;
using Xunit.Abstractions;

namespace Grynwald.ChangeLog.Test.ConventionalCommits
{
    /// <summary>
    /// Tests for <see cref="CommitMessageParser"/>
    /// </summary>
    public class CommitMessageParserTest
    {
        private readonly ITestOutputHelper m_OutputHelper;

        public CommitMessageParserTest(ITestOutputHelper outputHelper) => m_OutputHelper = outputHelper;


        public static IEnumerable<object[]> ValidParserTestCases()
        {
            static object[] TestCase(string id, string input, CommitMessage parsed)
            {
                return new object[] { id, input, new XunitSerializableCommitMessage(parsed) };
            }

            static object[] MultiLineTestCase(string id, CommitMessage parsed, params string[] input)
            {
                return new object[] { id, String.Join("\r\n", input), new XunitSerializableCommitMessage(parsed) };
            }

            var descriptions = new[]
            {
                "Some description",
                "Description: ",
                "Description ()",
                "Description!",
                "Description #",
                "Some description\r\n", // trailing line breaks are valid but ignored by the parser
                "Some description\n"    // trailing line breaks are valid but ignored by the parser
            };

            var i = 1;
            foreach (var descr in descriptions)
            {
                yield return TestCase(
                    $"T{i++:00}",
                    "feat: " + descr,
                    new CommitMessage(
                        header: new CommitMessageHeader(
                            type: new CommitType("feat"),
                            description: descr.TrimEnd('\r', '\n'),
                            scope: null,
                            isBreakingChange: false
                        ),
                        body: Array.Empty<string>(),
                        footers: Array.Empty<CommitMessageFooter>()
                    )
                );

                yield return TestCase(
                    $"T{i++:00}",
                    $"feat(scope): {descr}",
                    new CommitMessage(
                        header: new CommitMessageHeader(
                            type: new CommitType("feat"),
                            description: descr.TrimEnd('\r', '\n'),
                            scope: "scope",
                            isBreakingChange: false
                        ),
                        body: Array.Empty<string>(),
                        footers: Array.Empty<CommitMessageFooter>()
                    )
                );

                yield return TestCase(
                    $"T{i++:00}",
                    $"feat(scope)!: {descr}",
                    new CommitMessage(
                        header: new CommitMessageHeader(
                            type: new CommitType("feat"),
                            description: descr.TrimEnd('\r', '\n'),
                            scope: "scope",
                            isBreakingChange: true
                        ),
                        body: Array.Empty<string>(),
                        footers: Array.Empty<CommitMessageFooter>()
                    )
                );
            }

            // TODO: Ignore trailing blank lines

            // single line body
            yield return MultiLineTestCase(
                "T22",
                new CommitMessage(
                    header: new CommitMessageHeader(
                        type: new CommitType("type"),
                        description: "Description",
                        scope: "scope",
                        isBreakingChange: false
                    ),
                    body: new[] { "Single Line Body\r\n" },
                    footers: Array.Empty<CommitMessageFooter>()

                ),
                "type(scope): Description",
                "",
                "Single Line Body"
            );

            // multi-line body
            yield return MultiLineTestCase(
                "T23",
                new CommitMessage(
                    header: new CommitMessageHeader(
                        type: new CommitType("type"),
                        description: "Description",
                        scope: "scope",
                        isBreakingChange: false
                    ),
                    body: new[] { "Body line 1\r\nBody line 2\r\n" },
                    footers: Array.Empty<CommitMessageFooter>()
                ),
                "type(scope): Description",
                "",
                "Body line 1",
                "Body line 2"
            );


            //multi-paragraph body (1)
            yield return MultiLineTestCase(
                "T24",
                new CommitMessage(
                    header: new CommitMessageHeader(
                        type: new CommitType("type"),
                        description: "Description",
                        scope: "scope",
                        isBreakingChange: false
                    ),
                    body: new[]
                    {
                        "Body line 1.1\r\nBody line 1.2\r\n",
                        "Body line 2.1\r\nBody line 2.2\r\n"
                    },
                    footers: Array.Empty<CommitMessageFooter>()
                ),
                "type(scope): Description",
                    "",
                    "Body line 1.1",
                    "Body line 1.2",
                    "",
                    "Body line 2.1",
                    "Body line 2.2");

            // multi-paragraph body with trailing line break
            yield return MultiLineTestCase(
                "T25",
                new CommitMessage(
                    header: new CommitMessageHeader(
                        type: new CommitType("type"),
                        description: "Description",
                        scope: "scope",
                        isBreakingChange: false
                    ),
                    body: new[]
                    {
                        "Body line 1.1\r\nBody line 1.2\r\n",
                        "Body line 2.1\r\n"
                    },
                    footers: Array.Empty<CommitMessageFooter>()
                ),
                "type(scope): Description",
                "",
                "Body line 1.1",
                "Body line 1.2",
                "",
                "Body line 2.1\r\n"
            );


            // messages with footer
            i = 26;
            foreach (var footerType in new[] { "footer-type", "BREAKING CHANGE", "BREAKING-CHANGE" })
            {
                yield return MultiLineTestCase(
                        $"T{i++:00}",
                        new CommitMessage(
                            header: new CommitMessageHeader(
                                type: new CommitType("type"),
                                description: "Description",
                                scope: null,
                                isBreakingChange: false
                            ),
                            body: Array.Empty<string>(),
                            footers: new[]
                            {
                                new CommitMessageFooter(name: new CommitMessageFooterName(footerType), value: "Footer Description")
                            }
                        ),
                        "type: Description",
                        "",
                        $"{footerType}: Footer Description"
                    );

                yield return MultiLineTestCase(
                    $"T{i++:00}",
                    new CommitMessage(
                        header: new CommitMessageHeader(
                            type: new CommitType("type"),
                            description: "Description",
                            scope: null,
                            isBreakingChange: false
                        ),
                        body: Array.Empty<string>(),
                        footers: new[]
                        {
                            new CommitMessageFooter(name: new CommitMessageFooterName(footerType), value: "#Footer Description")
                        }
                    ),
                    "type: Description",
                    "",
                    $"{footerType} #Footer Description"
                );

            }


            yield return MultiLineTestCase(
                "T32",
                new CommitMessage(
                    header: new CommitMessageHeader(
                        type: new CommitType("type"),
                        description: "Description",
                        scope: null,
                        isBreakingChange: false
                    ),
                    body: Array.Empty<string>(),
                    footers: new[]
                    {
                        new CommitMessageFooter(name: new CommitMessageFooterName("Footer1"), value: "Footer Description1"),
                        new CommitMessageFooter(name: new CommitMessageFooterName("Footer2"), value: "#Footer Description2")
                    }
                ),
                "type: Description",
                "",
                "Footer1: Footer Description1",
                "Footer2 #Footer Description2"
            );

            // message with body AND footer
            yield return MultiLineTestCase(
                "T33",
                new CommitMessage(
                    header: new CommitMessageHeader(
                        type: new CommitType("type"),
                        description: "Description",
                        scope: "scope",
                        isBreakingChange: false
                    ),
                    body: new[]
                    {
                        "Body line 1.1\r\nBody line 1.2\r\n",
                        "Body line 2.1\r\n"
                    },
                    footers: new[]
                    {
                        new CommitMessageFooter(name: new CommitMessageFooterName("Reviewed-by"), value: "Z")
                    }
                ),
                "type(scope): Description",
                "",
                "Body line 1.1",
                "Body line 1.2",
                "",
                "Body line 2.1",
                "",
                "Reviewed-by: Z"
            );

            yield return MultiLineTestCase(
                "T34",
                new CommitMessage(
                    header: new CommitMessageHeader(
                        type: new CommitType("type"),
                        description: "Description",
                        scope: "scope",
                        isBreakingChange: false
                    ),
                    body: new[]
                    {
                            "Body line 1.1\r\nBody line 1.2\r\n",
                            "Body line 2.1\r\n"
                    },
                    footers: new[]
                    {
                        new CommitMessageFooter(name: new CommitMessageFooterName("Reviewed-by"), value: "Z"),
                        new CommitMessageFooter(name: new CommitMessageFooterName("Footer2"), value: "description")
                    }
                ),
                "type(scope): Description",
                "",
                "Body line 1.1",
                "Body line 1.2",
                "",
                "Body line 2.1",
                "",
                "Reviewed-by: Z",
                "Footer2: description"
            );
        }

        public static IEnumerable<object[]> InvalidParserTestCases()
        {
            static object[] TestCase(string id, int lineNumber, int columnNumber, string input)
            {
                return new object[] { id, lineNumber, columnNumber, input };
            }

            static object[] MultiLineTestCase(string id, int lineNumber, int columnNumber, params string[] input)
            {
                return new object[] { id, lineNumber, columnNumber, String.Join("\r\n", input) };
            }

            // empty
            yield return TestCase("T01", lineNumber: 1, columnNumber: 1, "");

            // Missing ': ' in header
            yield return TestCase("T02", lineNumber: 1, columnNumber: 5, "feat");

            // Incomplete scope / missing ')' in header
            yield return TestCase("T03", lineNumber: 1, columnNumber: 11, "feat(scope: Description");

            // missing description in header
            yield return TestCase($"T04", lineNumber: 1, columnNumber: 6, $"feat:");
            yield return TestCase($"T05", lineNumber: 1, columnNumber: 6, $"feat:\t");
            yield return TestCase($"T06", lineNumber: 1, columnNumber: 7, $"feat: ");
            yield return TestCase($"T07", lineNumber: 1, columnNumber: 7, $"feat:  ");

            yield return TestCase($"T08", lineNumber: 1, columnNumber: 13, $"feat(scope):");
            yield return TestCase($"T09", lineNumber: 1, columnNumber: 13, $"feat(scope):\t");
            yield return TestCase($"T10", lineNumber: 1, columnNumber: 14, $"feat(scope): ");
            yield return TestCase($"T11", lineNumber: 1, columnNumber: 14, $"feat(scope):  ");

            // missing description in footer            
            yield return TestCase($"T12", lineNumber: 3, columnNumber: 9, $"type(scope): Description\r\n\r\nFooter: ");
            yield return TestCase($"T13", lineNumber: 3, columnNumber: 18, $"type(scope): Description\r\n\r\nBREAKING CHANGE: ");
            yield return TestCase($"T14", lineNumber: 3, columnNumber: 9, $"type(scope): Description\r\n\r\nFooter: \t");
            yield return TestCase($"T15", lineNumber: 3, columnNumber: 9, $"type(scope): Description\r\n\r\nFooter:  ");
            yield return TestCase($"T16", lineNumber: 3, columnNumber: 8, $"type(scope): Description\r\n\r\nFooter # ");
            yield return TestCase($"T17", lineNumber: 3, columnNumber: 17, $"type(scope): Description\r\n\r\nBREAKING CHANGE #\t");

            // multiple blank lines between header and body
            yield return MultiLineTestCase(
                "T18",
                lineNumber: 3, columnNumber: 1,
                "type: Description",
                "",
                "",
                "Body"
            );

            // multiple blank lines between body and footer
            yield return MultiLineTestCase(
                "T19",
                lineNumber: 6, columnNumber: 1,
                "type: Description",
                "",
                "Body 1",
                "Body 2",
                "",
                "",
                "Footer: Value"
            );

            // footer with empty description
            yield return MultiLineTestCase(
                "T20",
                lineNumber: 3, columnNumber: 9,
                "type: Description",
                "",
                "Footer: "
            );

            yield return MultiLineTestCase(
                "T21",
                lineNumber: 3, columnNumber: 8,
                "type: Description",
                "",
                "Footer #"
            );

        }


        [Theory]
        [MemberData(nameof(ValidParserTestCases))]
        public void Parse_returns_expected_commit_message(string id, string commitMessage, XunitSerializableCommitMessage expected)
        {
            m_OutputHelper.WriteLine($"Test case {id}");

            var parsed = CommitMessageParser.Parse(commitMessage);
            Assert.Equal(expected.Value, parsed);
        }

        [Theory]
        [MemberData(nameof(InvalidParserTestCases))]
        public void Parse_throws_CommitMessageParserException_for_invalid_input(string id, int lineNumber, int columnNumber, string input)
        {
            m_OutputHelper.WriteLine($"Test case {id}");

            var ex = Assert.ThrowsAny<ParserException>(() => CommitMessageParser.Parse(input));
            Assert.Equal(lineNumber, ex.LineNumber);
            Assert.Equal(columnNumber, ex.ColumnNumber);
        }
    }
}
