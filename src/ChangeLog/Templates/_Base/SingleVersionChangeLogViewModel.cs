using System;
using System.Collections.Generic;
using System.Linq;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.ChangeLog.Model;

namespace Grynwald.ChangeLog.Templates
{
    internal class SingleVersionChangeLogViewModel
    {
        private readonly SingleVersionChangeLog m_Model;


        public string VersionDisplayName => m_Model.Version.Version.ToNormalizedString();

        public IEnumerable<ChangeLogEntryGroupViewModel> EntryGroups
        {
            get
            {
                {
                    var features = m_Model[CommitType.Feature];

                    if (features.Entries.Any())
                        yield return new ChangeLogEntryGroupViewModel(features);
                }
                {
                    var bugFixes = m_Model[CommitType.BugFix];

                    if (bugFixes.Entries.Any())
                        yield return new ChangeLogEntryGroupViewModel(bugFixes);
                }
            }
        }

        public IEnumerable<ChangeLogEntry> AllEntries
        {
            get
            {
                return m_Model[CommitType.Feature].Entries.Concat(m_Model[CommitType.BugFix].Entries).Concat(AdditionalBreakingChanges);
            }
        }

        public IEnumerable<ChangeLogEntry> AllBreakingChanges => m_Model.BreakingChanges;

        public IEnumerable<ChangeLogEntry> AdditionalBreakingChanges =>
            m_Model.BreakingChanges.Where(e => e.Type != CommitType.Feature && e.Type != CommitType.BugFix);


        public SingleVersionChangeLogViewModel(SingleVersionChangeLog model)
        {
            m_Model = model ?? throw new ArgumentNullException(nameof(model));
        }

    }
}
