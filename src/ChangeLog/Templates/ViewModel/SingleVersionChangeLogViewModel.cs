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


        public string VersionDisplayName { get; }

        public IReadOnlyList<ChangeLogEntryGroupViewModel> EntryGroups { get; }

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
            EntryGroups = LoadEntryGroups();
            BreakingChanges = LoadBreakingChanges();
        }


        private IReadOnlyList<ChangeLogEntryGroupViewModel> LoadEntryGroups()
        {
            var entryGroups = new List<ChangeLogEntryGroupViewModel>();

            var features = m_Model.AllEntries.Where(x => x.Type == CommitType.Feature).Select(GetViewModel);
            if (features.Any())
            {
                entryGroups.Add(new ChangeLogEntryGroupViewModel(m_Configuration, "New Features", features));
            }
            var bugfixes = m_Model.AllEntries.Where(x => x.Type == CommitType.BugFix).Select(GetViewModel);
            if (bugfixes.Any())
            {
                entryGroups.Add(new ChangeLogEntryGroupViewModel(m_Configuration, "Bug Fixes", bugfixes));
            }

            return entryGroups;
        }

        private IReadOnlyList<BreakingChangeViewModel> LoadBreakingChanges()
        {
            var breakingChanges = new List<BreakingChangeViewModel>();

            foreach (var entry in m_Model.AllEntries)
            {
                var entryViewModel = GetViewModel(entry);
                if (entry.BreakingChangeDescriptions.Any())
                {
                    foreach (var description in entry.BreakingChangeDescriptions)
                    {
                        breakingChanges.Add(new BreakingChangeViewModel(m_Configuration, description, entryViewModel));
                    }

                }
                else if (entry.ContainsBreakingChanges)
                {
                    breakingChanges.Add(new BreakingChangeViewModel(m_Configuration, entryViewModel.Title, entryViewModel));
                }
            }

            return breakingChanges;
        }

        private ChangeLogEntryViewModel GetViewModel(ChangeLogEntry model) =>
            m_ViewModels.GetOrAdd(model, () => new ChangeLogEntryViewModel(m_Configuration, model));
    }
}
