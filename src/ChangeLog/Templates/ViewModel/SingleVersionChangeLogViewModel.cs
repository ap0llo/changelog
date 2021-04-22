using System;
using System.Collections.Generic;
using System.Linq;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.ChangeLog.Model;

namespace Grynwald.ChangeLog.Templates.ViewModel
{
    internal class SingleVersionChangeLogViewModel
    {
        private readonly ChangeLogConfiguration m_Configuration;
        private readonly SingleVersionChangeLog m_Model;
        private readonly Dictionary<CommitType, ChangeLogConfiguration.EntryTypeConfiguration> m_EntryTypeConfiguration;


        public string VersionDisplayName => m_Model.Version.Version.ToNormalizedString();

        public IEnumerable<ChangeLogEntryGroupViewModel> EntryGroups
        {
            get
            {
                foreach (var (commitType, entries) in GetGroupedEntries())
                {
                    var displayName = m_EntryTypeConfiguration.GetValueOrDefault(commitType)?.DisplayName;
                    if (String.IsNullOrWhiteSpace(displayName))
                        displayName = commitType.Type;

                    yield return new ChangeLogEntryGroupViewModel(displayName, entries);
                }
            }
        }

        public IEnumerable<ChangeLogEntry> AllEntries
        {
            get
            {
                // Return entries in group order
                foreach (var (_, entries) in GetGroupedEntries())
                {
                    foreach (var entry in entries)
                    {
                        yield return entry;
                    }
                }
            }
        }

        public IEnumerable<ChangeLogEntry> BreakingChanges => m_Model.BreakingChanges;


        public SingleVersionChangeLogViewModel(ChangeLogConfiguration configuration, SingleVersionChangeLog model)
        {
            m_Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            m_Model = model ?? throw new ArgumentNullException(nameof(model));
            m_EntryTypeConfiguration = m_Configuration.EntryTypes.ToDictionary(kvp => new CommitType(kvp.Key), kvp => kvp.Value);
        }


        private IEnumerable<IGrouping<CommitType, ChangeLogEntry>> GetGroupedEntries()
        {
            return m_Model.AllEntries
               .GroupBy(x => x.Type)
               .OrderByDescending(group => m_EntryTypeConfiguration.GetValueOrDefault(group.Key)?.Priority ?? 0);
        }
    }
}
