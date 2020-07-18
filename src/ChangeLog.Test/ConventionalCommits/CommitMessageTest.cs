using System;
using System.Collections.Generic;
using Grynwald.ChangeLog.ConventionalCommits;

namespace Grynwald.ChangeLog.Test.ConventionalCommits
{
    /// <summary>
    /// Tests for <see cref="CommitMessage"/>
    /// </summary>
    public class CommitMessageTest : EqualityTest<CommitMessage, CommitMessageTest>, IEqualityTestDataProvider<CommitMessage>
    {
        public IEnumerable<(CommitMessage left, CommitMessage right)> GetEqualTestCases()
        {
            yield return (
                new CommitMessage(new CommitMessageHeader(CommitType.Feature, "Some Description")),
                new CommitMessage(new CommitMessageHeader(CommitType.Feature, "Some Description"))
            );

            yield return (
                new CommitMessage(
                    new CommitMessageHeader(CommitType.Feature, "Some Description"),
                    new[] { "Line1", "Line2" }),
                new CommitMessage(
                    new CommitMessageHeader(CommitType.Feature, "Some Description"),
                    new[] { "Line1", "Line2" })
            );

            yield return (
                new CommitMessage(
                    new CommitMessageHeader(CommitType.Feature, "Some Description"),
                    new[] { "Line1", "Line2" },
                    new[] { new CommitMessageFooter(new CommitMessageFooterName("name"), "value") }),
                new CommitMessage(
                    new CommitMessageHeader(CommitType.Feature, "Some Description"),
                    new[] { "Line1", "Line2" },
                    new[] { new CommitMessageFooter(new CommitMessageFooterName("name"), "value") })
            );
        }

        public IEnumerable<(CommitMessage left, CommitMessage right)> GetUnequalTestCases()
        {
            yield return (
                new CommitMessage(new CommitMessageHeader(CommitType.Feature, "Some Description")),
                new CommitMessage(new CommitMessageHeader(CommitType.BugFix, "Some Description"))
            );

            yield return (
                 new CommitMessage(new CommitMessageHeader(CommitType.Feature, "Some Description")),
                 new CommitMessage(new CommitMessageHeader(CommitType.Feature, "Some Other Description"))
             );


            yield return (
                new CommitMessage(
                    new CommitMessageHeader(CommitType.Feature, "Some Description"),
                    new[] { "Line1", "Line2" }),
                new CommitMessage(
                    new CommitMessageHeader(CommitType.Feature, "Some Description"),
                    new[] { "Line1", "Line2", "Line3" })
            );

            yield return (
                new CommitMessage(
                    new CommitMessageHeader(CommitType.Feature, "Some Description"),
                    new[] { "Line1", "Line2" },
                    new[]
                    {
                        new CommitMessageFooter(new CommitMessageFooterName("name"), "value")
                    }),
                new CommitMessage(
                    new CommitMessageHeader(CommitType.Feature, "Some Description"),
                    new[] { "Line1", "Line2" },
                    Array.Empty<CommitMessageFooter>())
            );

            yield return (
                new CommitMessage(
                    new CommitMessageHeader(CommitType.Feature, "Some Description"),
                    new[] { "Line1", "Line2" },
                    new[]
                    {
                        new CommitMessageFooter(new CommitMessageFooterName("name"), "value")
                    }),
                new CommitMessage(
                    new CommitMessageHeader(CommitType.Feature, "Some Description"),
                    new[] { "Line1", "Line2" },
                    new[]
                    {
                        new CommitMessageFooter(new CommitMessageFooterName("name"), "value"),
                        new CommitMessageFooter(new CommitMessageFooterName("name"), "value")
                    })
            );
        }
    }
}
