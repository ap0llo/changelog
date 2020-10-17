using System;
using System.Linq;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Model;
using Xunit.Abstractions;

namespace Grynwald.ChangeLog.Test.Model
{
    /// <summary>
    /// Wrapper class to make <see cref="ChangeLogEntry"/> serializable by xunit
    /// </summary>
    public sealed class XunitSerializableChangeLogEntry : IXunitSerializable
    {
        internal ChangeLogEntry Value { get; private set; }


        internal XunitSerializableChangeLogEntry(ChangeLogEntry value) => Value = value;


        [Obsolete("For use by Xunit only", true)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public XunitSerializableChangeLogEntry()
        { }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        public void Deserialize(IXunitSerializationInfo info)
        {
            var date = info.GetValue<DateTime>(nameof(ChangeLogEntry.Date));
            var type = info.GetValue<string>(nameof(ChangeLogEntry.Type));
            var scope = info.GetValue<string?>(nameof(ChangeLogEntry.Scope));
            var containsBreakingChanges = info.GetValue<bool>(nameof(ChangeLogEntry.ContainsBreakingChanges));
            var summary = info.GetValue<string>(nameof(ChangeLogEntry.Summary));
            var body = info.GetValue<string[]>(nameof(ChangeLogEntry.Body));
            var breakingChangeDescriptions = info.GetValue<string[]>(nameof(ChangeLogEntry.BreakingChangeDescriptions));
            var footers = info.GetValue<XunitSerializableChangeLogEntryFooter[]>(nameof(ChangeLogEntry.Footers));
            var commitId = info.GetValue<string>($"{nameof(ChangeLogEntry.Commit)}.{nameof(ChangeLogEntry.Commit.Id)}");
            var abbreviatedCommitId = info.GetValue<string>($"{nameof(ChangeLogEntry.Commit)}.{nameof(ChangeLogEntry.Commit.AbbreviatedId)}");

            Value = new ChangeLogEntry(
                date,
                new CommitType(type),
                scope,
                containsBreakingChanges,
                summary,
                body,
                footers.Select(x => x.Value).ToArray(),
                breakingChangeDescriptions,
                new GitId(commitId, abbreviatedCommitId)
            );
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(Value.Date), Value.Date);
            info.AddValue(nameof(Value.Type), Value.Type.Type);
            info.AddValue(nameof(Value.Scope), Value.Scope);
            info.AddValue(nameof(Value.ContainsBreakingChanges), Value.ContainsBreakingChanges);
            info.AddValue(nameof(Value.Summary), Value.Summary);
            info.AddValue(nameof(Value.Body), Value.Body.ToArray());
            info.AddValue(nameof(Value.BreakingChangeDescriptions), Value.BreakingChangeDescriptions.ToArray());
            info.AddValue(nameof(Value.Footers), Value.Footers.Select(XunitSerializableChangeLogEntryFooter.Wrap).ToArray());
            info.AddValue($"{nameof(Value.Commit)}.{nameof(Value.Commit.Id)}", Value.Commit.Id);
            info.AddValue($"{nameof(Value.Commit)}.{nameof(Value.Commit.AbbreviatedId)}", Value.Commit.AbbreviatedId);
        }
    }
}
