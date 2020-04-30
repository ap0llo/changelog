using System;
using System.Collections.Generic;
using System.Linq;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.ChangeLog.Model;
using Grynwald.Utilities.Collections;

namespace Grynwald.ChangeLog.Templates.ViewModel
{
    internal class SingleVersionChangeLogViewModel
    {
        private readonly Dictionary<ChangeLogEntry, ChangeLogEntryViewModel> m_ViewModels = new Dictionary<ChangeLogEntry, ChangeLogEntryViewModel>();


        public string VersionDisplayName { get; }

        public IReadOnlyList<ChangeLogEntryGroupViewModel> EntryGroups { get; }

        public IReadOnlyList<BreakingChangeViewModel> BreakingChanges { get; }


        /// <summary>
        /// Initializes a new instance of <see cref="SingleVersionChangeLog"/>
        /// </summary>
        /// <param name="model">The view model's underlying model</param>
        public SingleVersionChangeLogViewModel(SingleVersionChangeLog model)
        {
            if (model is null)
                throw new ArgumentNullException(nameof(model));

            VersionDisplayName = model.Version.Version.ToNormalizedString();
            EntryGroups = LoadEntryGroups(model);
            BreakingChanges = LoadBreakingChanges(model);
        }


        private IReadOnlyList<ChangeLogEntryGroupViewModel> LoadEntryGroups(SingleVersionChangeLog model)
        {
            var entryGroups = new List<ChangeLogEntryGroupViewModel>();

            var features = model.AllEntries.Where(x => x.Type == CommitType.Feature).Select(GetViewModel);
            if (features.Any())
            {
                entryGroups.Add(new ChangeLogEntryGroupViewModel("New Features", features));
            }
            var bugfixes = model.AllEntries.Where(x => x.Type == CommitType.BugFix).Select(GetViewModel);
            if (bugfixes.Any())
            {
                entryGroups.Add(new ChangeLogEntryGroupViewModel("Bug Fixes", bugfixes));
            }

            return entryGroups;
        }

        private IReadOnlyList<BreakingChangeViewModel> LoadBreakingChanges(SingleVersionChangeLog model)
        {
            var breakingChanges = new List<BreakingChangeViewModel>();

            foreach (var entry in model.AllEntries)
            {
                var entryViewModel = GetViewModel(entry);
                if (entry.BreakingChangeDescriptions.Any())
                {
                    foreach (var description in entry.BreakingChangeDescriptions)
                    {
                        breakingChanges.Add(new BreakingChangeViewModel(description, entryViewModel));
                    }

                }
                else if (entry.ContainsBreakingChanges)
                {
                    breakingChanges.Add(new BreakingChangeViewModel(entryViewModel.Title, entryViewModel));
                }
            }

            return breakingChanges;
        }

        private ChangeLogEntryViewModel GetViewModel(ChangeLogEntry model) =>
            m_ViewModels.GetOrAdd(model, () => new ChangeLogEntryViewModel(model));
    }
}
