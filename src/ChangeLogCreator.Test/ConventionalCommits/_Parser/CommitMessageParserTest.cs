﻿using System;
using System.Collections.Generic;
using System.Linq;
using ChangeLogCreator.ConventionalCommits;
using Xunit;

namespace ChangeLogCreator.Test.ConventionalCommits
{
    /// <summary>
    /// Tests for <see cref="CommitMessageParser"/>
    /// </summary>
    public class CommitMessageParserTest
    {
        public static IEnumerable<object[]> ValidParserTestCases()
        {
            static object[] TestCase(string input, CommitMessage parsed) => new object[] { input, new XunitSerializableCommitMessage(parsed) };

            static object[] MultiLineTestCase(CommitMessage parsed, params string[] input) => new object[] { String.Join("\r\n", input), new XunitSerializableCommitMessage(parsed) };

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

            foreach (var descr in descriptions)
            {
                ;

                yield return TestCase(
                    "feat: " + descr,
                    new CommitMessage()
                    {
                        Header = new CommitMessageHeader()
                        {
                            Type = "feat",
                            Description = descr.TrimEnd('\r', '\n'),
                        }
                    }
                );

                yield return TestCase(
                    $"feat(scope): {descr}",
                    new CommitMessage()
                    {
                        Header = new CommitMessageHeader()
                        {
                            Type = "feat",
                            Scope = "scope",
                            Description = descr.TrimEnd('\r', '\n'),
                        }
                    }
                );

                yield return TestCase(
                    $"feat(scope)!: {descr}",
                    new CommitMessage()
                    {
                        Header = new CommitMessageHeader()
                        {
                            Type = "feat",
                            Scope = "scope",
                            Description = descr.TrimEnd('\r', '\n'),
                            IsBreakingChange = true
                        }
                    }
                );
            }

            // TODO: Ignore trailing blank lines

            // single line body
            yield return MultiLineTestCase(
                new CommitMessage()
                {
                    Header = new CommitMessageHeader()
                    {
                        Type = "type",
                        Scope = "scope",
                        Description = "Description",
                        IsBreakingChange = false,
                    },
                    Body = new[] { "Single Line Body\r\n" },
                    Footers = Array.Empty<CommitMessageFooter>()

                },
                "type(scope): Description",
                "",
                "Single Line Body"
            );

            // multi-line body
            yield return MultiLineTestCase(
                new CommitMessage()
                {
                    Header = new CommitMessageHeader()
                    {
                        Type = "type",
                        Scope = "scope",
                        Description = "Description",
                        IsBreakingChange = false,
                    },
                    Body = new[] { "Body line 1\r\nBody line 2\r\n" },
                    Footers = Array.Empty<CommitMessageFooter>()
                },
                "type(scope): Description",
                "",
                "Body line 1",
                "Body line 2"
            );


            //multi-paragraph body (1)
            yield return MultiLineTestCase(
                new CommitMessage()
                {
                    Header = new CommitMessageHeader()
                    {
                        Type = "type",
                        Scope = "scope",
                        Description = "Description",
                        IsBreakingChange = false,
                    },
                    Body = new[]
                    {
                        "Body line 1.1\r\nBody line 1.2\r\n",
                        "Body line 2.1\r\nBody line 2.2\r\n"
                    },
                    Footers = Array.Empty<CommitMessageFooter>()
                },
                "type(scope): Description",
                    "",
                    "Body line 1.1",
                    "Body line 1.2",
                    "",
                    "Body line 2.1",
                    "Body line 2.2");

            // multi-paragraph body with trailing line break
            yield return MultiLineTestCase(
                new CommitMessage()
                {
                    Header = new CommitMessageHeader()
                    {
                        Type = "type",
                        Scope = "scope",
                        Description = "Description",
                        IsBreakingChange = false,
                    },
                    Body = new[]
                    {
                        "Body line 1.1\r\nBody line 1.2\r\n",
                        "Body line 2.1\r\n"
                    },
                    Footers = Array.Empty<CommitMessageFooter>()
                },
                "type(scope): Description",
                "",
                "Body line 1.1",
                "Body line 1.2",
                "",
                "Body line 2.1\r\n"
            );


            // messages with footer
            foreach (var footerType in new[] { "footer-type", "BREAKING CHANGE", "BREAKING-CHANGE" })
            {
                foreach (var separator in new[] { ": ", " #" })
                {
                    yield return MultiLineTestCase(
                        new CommitMessage()
                        {
                            Header = new CommitMessageHeader()
                            {
                                Type = "type",
                                Scope = null,
                                Description = "Description",
                                IsBreakingChange = false,
                            },
                            Body = Array.Empty<string>(),
                            Footers = new[]
                            {
                                new CommitMessageFooter() { Type = footerType, Description = "Footer Description" }
                            }
                        },
                        "type: Description",
                        "",
                        $"{footerType}{separator}Footer Description"
                    );
                }
            }


            yield return MultiLineTestCase(
                new CommitMessage()
                {
                    Header = new CommitMessageHeader()
                    {
                        Type = "type",
                        Scope = null,
                        Description = "Description",
                        IsBreakingChange = false,
                    },
                    Body = Array.Empty<string>(),
                    Footers = new[]
                    {
                        new CommitMessageFooter() { Type = "Footer1", Description = "Footer Description1" },
                        new CommitMessageFooter() { Type = "Footer2", Description = "Footer Description2" }
                    }
                },
                "type: Description",
                "",
                "Footer1: Footer Description1",
                "Footer2 #Footer Description2"
            );

            // message with body AND footer
            yield return MultiLineTestCase(
                new CommitMessage()
                {
                    Header = new CommitMessageHeader()
                    {
                        Type = "type",
                        Scope = "scope",
                        Description = "Description",
                        IsBreakingChange = false,
                    },
                    Body = new[]
                    {
                        "Body line 1.1\r\nBody line 1.2\r\n",
                        "Body line 2.1\r\n"
                    },
                    Footers = new[]
                    {
                        new CommitMessageFooter() { Type = "Reviewed-by", Description = "Z" }
                    }
                },
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
                new CommitMessage()
                {
                    Header = new CommitMessageHeader()
                    {
                        Type = "type",
                        Scope = "scope",
                        Description = "Description",
                        IsBreakingChange = false,
                    },
                    Body = new[]
                    {
                            "Body line 1.1\r\nBody line 1.2\r\n",
                            "Body line 2.1\r\n"
                    },
                    Footers = new[]
                    {
                        new CommitMessageFooter() { Type = "Reviewed-by", Description = "Z" },    
                        new CommitMessageFooter() { Type = "Footer2", Description = "description" }
                    }
                },
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
            static object[] TestCase(string input) => new object[] { input };

            static object[] MultiLineTestCase(params string[] input) => new object[] { String.Join("\r\n", input) };

            // empty
            yield return TestCase("");

            // Missing ': ' in header
            yield return TestCase("feat");

            // Incomplete scope / missing ')' in header
            yield return TestCase("feat(scope: Description");

            // missing description in header
            foreach (var invalidDescription in new[] { "", "\t", " ", "  " })
            {
                yield return TestCase($"feat:{invalidDescription}");
                yield return TestCase($"feat(scope):{invalidDescription}");
            }

            // missing description in footer
            foreach (var invalidFooter in new[] { "Footer: ", "BREAKING CHANGE: ", "Footer: \t", "Footer:  ", "Footer # ", "BREAKING CHANGE #\t" })
            {
                yield return TestCase($"type(scope): Description\r\n\r\n{invalidFooter}");
            }

            // multiple blank lines between header and body
            yield return MultiLineTestCase(
                    "type: Description",
                    "",
                    "",
                    "Body"
                );

            // multiple blank lines between body and footer
            yield return MultiLineTestCase(
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
                    "type: Description",
                    "",
                    "Footer: "
                );

            yield return MultiLineTestCase(
                    "type: Description",
                    "",
                    "Footer #"
                );

        }


        [Theory]
        [MemberData(nameof(ValidParserTestCases))]
        public void Parse_returns_expected_commit_message(string commitMessage, XunitSerializableCommitMessage expected)
        {
            var parsed = CommitMessageParser.Parse(commitMessage);
            Assert.Equal(expected.Value, parsed);
        }

        [Theory]
        [MemberData(nameof(InvalidParserTestCases))]
        public void Parse_throws_CommitMessageParserException_for_invalid_input(string input)
        {
            Assert.ThrowsAny<CommitMessageParserException>(() => CommitMessageParser.Parse(input));
            //TODO: Check exception includes information about position where the error occurred
        }
    }
}
