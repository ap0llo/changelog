using System;
using System.Collections.Generic;
using System.Linq;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.ChangeLog.Model;
using Grynwald.Utilities.Collections;

namespace Grynwald.ChangeLog.Templates.ViewModel
{
    internal class SingleVersionChangeLogViewModel
    {
        private readonly Dictionary<ChangeLogEntry, ChangeLogEntryViewModel> m_ViewModels = new Dictionary<ChangeLogEntry, ChangeLogEntryViewModel>();
        private readonly ChangeLogConfiguration m_Configuration;
        private readonly SingleVersionChangeLog m_Model;

        /// <summary>
        /// Gets the display name of the version this change log describes 
        /// </summary>
        public string VersionDisplayName { get; }

        /// <summary>
        /// Gets all entries for the change log
        /// </summary>
        public IReadOnlyList<ChangeLogEntryViewModel> AllEntries { get; }

        /// <summary>
        /// Gets the entry groups for the current change log.
        /// A entry group, groups changes of the same type (e.g. New Features or Bug Fixes)
        /// </summary>
        public IReadOnlyList<ChangeLogEntryGroupViewModel> EntryGroups { get; }

        /// <summary>
        /// Gets all breaking changes introduced in this version
        /// </summary>
        public IReadOnlyList<BreakingChangeViewModel> BreakingChanges { get; }


        /// <summary>
        /// Initializes a new instance of <see cref="SingleVersionChangeLog"/>
        /// </summary>
        /// <param name="model">The view model's underlying model</param>
        public SingleVersionChangeLogViewModel(ChangeLogConfiguration configuration, SingleVersionChangeLog model)
        {
            m_Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            m_Model = model ?? throw new ArgumentNullException(nameof(model));

            VersionDisplayName = model.Version.Version.ToNormalizedString();

            AllEntries = model.AllEntries
                .Where(IsIncludedEntry)
                .OrderBy(x => x.Date)
                .Select(GetViewModel).ToArray();

            EntryGroups = LoadEntryGroups(AllEntries);
            BreakingChanges = AllEntries.SelectMany(x => x.BreakingChanges).OrderBy(x => x.Entry.Date).ToArray();
        }


        private IReadOnlyList<ChangeLogEntryGroupViewModel> LoadEntryGroups(IEnumerable<ChangeLogEntryViewModel> entries)
        {
            var entryGroups = new List<ChangeLogEntryGroupViewModel>();

            var features = entries.Where(x => x.Type == CommitType.Feature);
            if (features.Any())
            {
                entryGroups.Add(new ChangeLogEntryGroupViewModel(m_Configuration, "New Features", features));
            }
            var bugfixes = entries.Where(x => x.Type == CommitType.BugFix);
            if (bugfixes.Any())
            {
                entryGroups.Add(new ChangeLogEntryGroupViewModel(m_Configuration, "Bug Fixes", bugfixes));
            }

            return entryGroups;
        }

        private ChangeLogEntryViewModel GetViewModel(ChangeLogEntry model) =>
            m_ViewModels.GetOrAdd(model, () => new ChangeLogEntryViewModel(m_Configuration, model));

        private bool IsIncludedEntry(ChangeLogEntry entry) =>
            entry.Type == CommitType.BugFix ||
            entry.Type == CommitType.Feature ||
            entry.ContainsBreakingChanges;
    }
}
