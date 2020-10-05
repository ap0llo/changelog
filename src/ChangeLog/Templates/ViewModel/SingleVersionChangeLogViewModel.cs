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
        private readonly Dictionary<CommitType, ChangeLogConfiguration.EntryTypeConfiguration> m_Types;



        public string VersionDisplayName => m_Model.Version.Version.ToNormalizedString();

        public IEnumerable<ChangeLogEntryGroupViewModel> EntryGroups
        {
            get
            {
                foreach (var (type, configuration) in m_Types)
                {
                    var displayName = String.IsNullOrWhiteSpace(configuration.DisplayName)
                        ? type.Type
                        : configuration.DisplayName;

                    var entryGroup = m_Model[type];
                    if (entryGroup.Entries.Count > 0)
                        yield return new ChangeLogEntryGroupViewModel(displayName, entryGroup);
                }
            }
        }

        public IEnumerable<ChangeLogEntry> AllEntries
        {
            get
            {
                foreach (var type in m_Types.Keys)
                {
                    foreach (var entry in m_Model[type].Entries)
                    {
                        yield return entry;
                    }
                }
                foreach (var entry in AdditionalBreakingChanges)
                {
                    yield return entry;
                }
            }
        }

        public IEnumerable<ChangeLogEntry> AllBreakingChanges => m_Model.BreakingChanges;

        public IEnumerable<ChangeLogEntry> AdditionalBreakingChanges =>
            m_Model.BreakingChanges.Where(e => !m_Types.ContainsKey(e.Type));


        public SingleVersionChangeLogViewModel(ChangeLogConfiguration configuration, SingleVersionChangeLog model)
        {
            m_Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            m_Model = model ?? throw new ArgumentNullException(nameof(model));
            m_Types = m_Configuration.EntryTypes.ToDictionary(kvp => new CommitType(kvp.Key), kvp => kvp.Value);
        }
    }
}
