using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Grynwald.ChangeLog.Git;
using Grynwald.Utilities.Collections;

namespace Grynwald.ChangeLog.Model
{
    public sealed class SingleVersionChangeLog : IEnumerable<ChangeLogEntry>
    {
        private readonly List<ChangeLogEntry> m_AllEntries = new List<ChangeLogEntry>();
        private readonly Dictionary<GitId, GitCommit> m_AllCommits = new Dictionary<GitId, GitCommit>();


        public VersionInfo Version { get; }

        public IEnumerable<ChangeLogEntry> AllEntries => m_AllEntries.OrderBy(x => x.Date);

        public IReadOnlyCollection<GitCommit> AllCommits { get; }

        /// <summary>
        /// Gets all change log entries that contain a breaking change.
        /// </summary>
        public IEnumerable<ChangeLogEntry> BreakingChanges => AllEntries.Where(e => e.ContainsBreakingChanges);


        /// <summary>
        /// Initializes a new instance of <see cref="SingleVersionChangeLog"/>
        /// </summary>
        public SingleVersionChangeLog(VersionInfo version)
        {
            Version = version ?? throw new ArgumentNullException(nameof(version));
            AllCommits = ReadOnlyCollectionAdapter.Create(m_AllCommits.Values);
        }


        public void Add(ChangeLogEntry entry)
        {
            if (entry is null)
                throw new ArgumentNullException(nameof(entry));

            m_AllEntries.Add(entry);
        }

        public void Remove(ChangeLogEntry entry)
        {
            if (entry is null)
                throw new ArgumentNullException(nameof(entry));

            m_AllEntries.Remove(entry);
        }

        public void Add(GitCommit commit)
        {
            if (commit is null)
                throw new ArgumentNullException(nameof(commit));

            if (m_AllCommits.ContainsKey(commit.Id))
                throw new InvalidOperationException($"Changelog already contains commit '{commit.Id}'");

            m_AllCommits.Add(commit.Id, commit);
        }

        public void Remove(GitCommit commit)
        {
            if (commit is null)
                throw new ArgumentNullException(nameof(commit));

            m_AllCommits.Remove(commit.Id);
        }

        /// <inheritdoc />
        public IEnumerator<ChangeLogEntry> GetEnumerator() => AllEntries.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => AllEntries.GetEnumerator();
    }
}
