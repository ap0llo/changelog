using System;
using System.Collections.Generic;
using System.Linq;
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
            static object[] TestCase(string id, string input, bool strictMode, CommitMessage parsed)
            {
                return new object[] { id, input, strictMode, new XunitSerializableCommitMessage(parsed) };
            }

            static object[] MultiLineTestCase(string id, CommitMessage parsed, bool strictMode, params string[] input)
            {
                return new object[] { id, String.Join("", input.Select(x => String.Concat(x, "\r\n"))), false, new XunitSerializableCommitMessage(parsed) };
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
                    strictMode: true,
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
                    strictMode: true,
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
                    strictMode: true,
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
                strictMode: true,
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
                strictMode: true,
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
                strictMode: true,
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
                strictMode: true,
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
                        strictMode: true,
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
                    strictMode: true,
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
                strictMode: true,
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
                strictMode: true,
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
                strictMode: true,
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

            //
            // Trailing blank lines are ignored (whitespace-only lines are treated as blank lines as well)
            // (only allowed when strictMode = false)
            //

            yield return MultiLineTestCase(
                "T35",
                new CommitMessage(new CommitMessageHeader(CommitType.Feature, "Some Description")),
                strictMode: false,
                "feat: Some Description",
                "  ",
                ""
            );

            yield return MultiLineTestCase(
                 "T36",
                 new CommitMessage(
                     header: new CommitMessageHeader(CommitType.Feature, "Some Description"),
                     body: new[] { "Message Body\r\n" }
                 ),
                 strictMode: false,
                 "feat: Some Description",
                 "  ",
                 "Message Body",
                 "",
                 "  "
             );

            yield return MultiLineTestCase(
                 "T37",
                 new CommitMessage(
                     header: new CommitMessageHeader(CommitType.Feature, "Some Description"),
                     body: new[] { "Message Body\r\n" },
                     footers: new[] { new CommitMessageFooter(new CommitMessageFooterName("name"), "value") }
                 ),
                 strictMode: false,
                 "feat: Some Description",
                 "  ",
                 "Message Body",
                 "",
                 "name: value",
                 "",
                 "  "
             );


            //
            // Multiple blank lines between sections
            // (only allowed when strictMode = false)
            //

            // multiple blank lines between header and body
            yield return MultiLineTestCase(
                 "T38",
                 new CommitMessage(
                     header: new CommitMessageHeader(CommitType.Feature, "Some Description"),
                     body: new[] { "Message Body\r\n" }
                 ),
                 strictMode: false,
                 "feat: Some Description",
                 "  ",
                 "",
                 "\t",
                 "Message Body"
             );

            // multiple blank lines between paragraphs
            yield return MultiLineTestCase(
                 "T39",
                 new CommitMessage(
                     header: new CommitMessageHeader(CommitType.Feature, "Some Description"),
                     body: new[] { "Line1\r\n", "Line2\r\n" }
                 ),
                 strictMode: false,
                 "feat: Some Description",
                 "",
                 "Line1",
                 "  ",
                 "",
                 "\t",
                 "Line2"
             );

            // multiple blank lines between body and footers
            yield return MultiLineTestCase(
                 "T40",
                 new CommitMessage(
                     header: new CommitMessageHeader(CommitType.Feature, "Some Description"),
                     body: new[] { "Message Body\r\n" },
                     footers: new[] { new CommitMessageFooter(new CommitMessageFooterName("name"), "value") }
                 ),
                 strictMode: false,
                 "feat: Some Description",
                 "",
                 "Message Body",
                 "  ",
                 "",
                 "\t",
                 "name: value"
             );

            //
            // Blank lines between footers
            // (only allowed when strictMode = false)
            //
            yield return MultiLineTestCase(
                "T41",
                new CommitMessage(
                    header: new CommitMessageHeader(
                        type: new CommitType("type"),
                        description: "Description",
                        scope: null,
                        isBreakingChange: false
                    ),
                    body: new[] { "Message Body\r\n" },
                    footers: new[]
                    {
                        new CommitMessageFooter(name: new CommitMessageFooterName("Footer1"), value: "Footer Description1"),
                        new CommitMessageFooter(name: new CommitMessageFooterName("Footer2"), value: "Footer Description2"),
                        new CommitMessageFooter(name: new CommitMessageFooterName("Footer3"), value: "Footer Description3"),
                        new CommitMessageFooter(name: new CommitMessageFooterName("Footer4"), value: "Footer Description4")
                    }
                ),
                strictMode: false,
                "type: Description",
                "",
                "Message Body",
                "",
                "Footer1: Footer Description1",
                "",
                "Footer2: Footer Description2",
                "Footer3: Footer Description3",
                "",
                "\t",
                " ",
                "Footer4: Footer Description4"
            );

            // Parser retains whitespace in footer values
            yield return MultiLineTestCase(
                "T42",
                new CommitMessage(
                    header: new CommitMessageHeader(
                        type: new CommitType("type"),
                        description: "Description",
                        scope: null,
                        isBreakingChange: false
                    ),
                    body: new[] { "Message Body\r\n" },
                    footers: new[]
                    {
                        new CommitMessageFooter(name: new CommitMessageFooterName("Footer1"), value: "Some Value with trailing whitespace   \t"),
                        new CommitMessageFooter(name: new CommitMessageFooterName("Footer2"), value: "  Some Value with leading whitespace"),
                    }
                ),
                strictMode: false,
                "type: Description",
                "",
                "Message Body",
                "",
                "Footer1: Some Value with trailing whitespace   \t",
                "Footer2:   Some Value with leading whitespace"
            );
        }

        public static IEnumerable<object[]> InvalidParserTestCases()
        {
            static object[] TestCase(string id, int lineNumber, int columnNumber, bool strictMode, string input)
            {
                return new object[] { id, lineNumber, columnNumber, input, strictMode };
            }

            static object[] MultiLineTestCase(string id, int lineNumber, int columnNumber, bool strictMode, params string[] input)
            {
                return new object[] { id, lineNumber, columnNumber, String.Join("", input.Select(x => String.Concat(x, "\r\n"))), strictMode };
            }

            // empty
            yield return TestCase("T01", lineNumber: 1, columnNumber: 1, strictMode: true, "");

            // Missing ': ' in header
            yield return TestCase("T02", lineNumber: 1, columnNumber: 5, strictMode: true, "feat");

            // Incomplete scope / missing ')' in header
            yield return TestCase("T03", lineNumber: 1, columnNumber: 11, strictMode: true, "feat(scope: Description");

            // missing description in header
            yield return TestCase($"T04", lineNumber: 1, columnNumber: 6, strictMode: true, $"feat:");
            yield return TestCase($"T05", lineNumber: 1, columnNumber: 6, strictMode: true, $"feat:\t");
            yield return TestCase($"T06", lineNumber: 1, columnNumber: 7, strictMode: true, $"feat: ");
            yield return TestCase($"T07", lineNumber: 1, columnNumber: 7, strictMode: true, $"feat:  ");

            yield return TestCase($"T08", lineNumber: 1, columnNumber: 13, strictMode: true, $"feat(scope):");
            yield return TestCase($"T09", lineNumber: 1, columnNumber: 13, strictMode: true, $"feat(scope):\t");
            yield return TestCase($"T10", lineNumber: 1, columnNumber: 14, strictMode: true, $"feat(scope): ");
            yield return TestCase($"T11", lineNumber: 1, columnNumber: 14, strictMode: true, $"feat(scope):  ");

            // missing description in footer            
            yield return TestCase($"T12", lineNumber: 3, columnNumber: 9, strictMode: true, "type(scope): Description\r\n\r\nFooter: ");
            yield return TestCase($"T13", lineNumber: 3, columnNumber: 18, strictMode: true, "type(scope): Description\r\n\r\nBREAKING CHANGE: ");
            yield return TestCase($"T14", lineNumber: 3, columnNumber: 9, strictMode: true, "type(scope): Description\r\n\r\nFooter: \t");
            yield return TestCase($"T15", lineNumber: 3, columnNumber: 9, strictMode: true, "type(scope): Description\r\n\r\nFooter:  ");
            yield return TestCase($"T16", lineNumber: 3, columnNumber: 8, strictMode: true, "type(scope): Description\r\n\r\nFooter # ");
            yield return TestCase($"T17", lineNumber: 3, columnNumber: 17, strictMode: true, "type(scope): Description\r\n\r\nBREAKING CHANGE #\t");

            // footer with empty description
            yield return MultiLineTestCase(
                "T20",
                lineNumber: 3, columnNumber: 9,
                strictMode: true,
                "type: Description",
                "",
                "Footer: "
            );

            yield return MultiLineTestCase(
                "T21",
                lineNumber: 3, columnNumber: 8,
                strictMode: true,
                "type: Description",
                "",
                "Footer #"
            );

            //
            // Trailing blank lines are an error when strictMode is true (allowed when strictMode = false)
            //
            yield return MultiLineTestCase(
                "T22",
                lineNumber: 3, columnNumber: 1,
                strictMode: true,
                "feat: Some Description",
                "",
                ""
            );
            yield return MultiLineTestCase(
                "T23",
                lineNumber: 5, columnNumber: 1,
                strictMode: true,
                "feat: Some Description",
                "",
                "Message Body",
                "",
                ""
            );

            yield return MultiLineTestCase(
                "T24",
                lineNumber: 7, columnNumber: 1,
                strictMode: true,
                "feat: Some Description",
                "",
                "Message Body",
                "",
                "name: value",
                "",
                ""
             );

            // Whitespace-only lines are not treated as blank lines in strictMode
            yield return MultiLineTestCase(
                "T25",
                lineNumber: 2, columnNumber: 1,
                strictMode: true,
                "feat: Some Description",
                "  ",
                "footer: value"
            );


            //
            // Multiple blank lines between sections are an error in strict mode
            // (only allowed when strictMode = false)
            //

            // multiple blank lines between header and body
            yield return MultiLineTestCase(
                "T26",
                lineNumber: 3, columnNumber: 1,
                strictMode: true,
                "feat: Some Description",
                "",
                "",
                "Message Body"
            );

            // multiple blank lines between paragraphs
            yield return MultiLineTestCase(
                "T27",
                lineNumber: 5, columnNumber: 1,
                strictMode: true,
                "feat: Some Description",
                "",
                "Line1",
                "",
                "",
                "",
                "Line2"
            );

            // multiple blank lines between body and footers
            yield return MultiLineTestCase(
                "T28",
                lineNumber: 5, columnNumber: 1,
                strictMode: true,
                "feat: Some Description",
                "",
                "Message Body",
                "",
                "",
                "",
                "name: value"
            );

            //
            // Blank lines between footers
            //
            yield return MultiLineTestCase(
                "T29",
                lineNumber: 7, columnNumber: 1,
                strictMode: true,
                "type: Description",
                "",
                "Message Body",
                "",
                "Footer1: Footer Description1",
                "",
                "Footer2: Footer Description2"
            );
        }


        [Fact]
        public void Parse_checks_arguments_for_null()
        {
            Assert.Throws<ArgumentNullException>(() => CommitMessageParser.Parse(null!, true));
            Assert.Throws<ArgumentNullException>(() => CommitMessageParser.Parse(null!, false));
        }

        [Theory]
        [MemberData(nameof(ValidParserTestCases))]
        public void Parse_returns_expected_commit_message(string id, string commitMessage, bool strictMode, XunitSerializableCommitMessage expected)
        {
            m_OutputHelper.WriteLine($"Test case {id}");

            var parsed = CommitMessageParser.Parse(commitMessage, strictMode);
            Assert.Equal(expected.Value, parsed);
        }

        [Theory]
        [MemberData(nameof(InvalidParserTestCases))]
        public void Parse_throws_CommitMessageParserException_for_invalid_input(string id, int lineNumber, int columnNumber, string input, bool strictMode)
        {
            m_OutputHelper.WriteLine($"Test case {id}");

            var ex = Assert.ThrowsAny<ParserException>(() => CommitMessageParser.Parse(input, strictMode));
            m_OutputHelper.WriteLine($"Exception Message: {ex.Message}");
            Assert.Equal(lineNumber, ex.LineNumber);
            Assert.Equal(columnNumber, ex.ColumnNumber);
        }
    }
}
