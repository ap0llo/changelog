using System;
using System.Collections.Generic;
using System.Linq;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.ChangeLog.Model;

namespace Grynwald.ChangeLog.Templates.ViewModel
{
    internal class ChangeLogEntryViewModel
    {
        private readonly ChangeLogConfiguration m_Configuration;
        private readonly ChangeLogEntry m_Model;


        public string Title { get; }

        public DateTime Date { get; }

        public IReadOnlyList<string> Body { get; }

        public IReadOnlyList<ChangeLogEntryFooterViewModel> Footers { get; }


        public ChangeLogEntryViewModel(ChangeLogConfiguration configuration, ChangeLogEntry model)
        {
            m_Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            m_Model = model ?? throw new ArgumentNullException(nameof(model));

            var scopeDisplayName = GetScopeDisplayName();
            Title = String.IsNullOrEmpty(scopeDisplayName) ? model.Summary : $"{scopeDisplayName}: {model.Summary}";
            Date = model.Date;
            Body = model.Body;

            // Load all footers except "BREAKING CHANGES" footers. They are handled separately
            var footers = model.Footers
                .Where(x => x.Name != CommitMessageFooterName.BreakingChange)
                .Select(x => new ChangeLogEntryFooterViewModel(configuration, x))
                .ToList();

            // The commit id (and web link) is included in the changelog as a implicit footer
            footers.Add(new ChangeLogEntryFooterViewModel(configuration, "Commit", model.Commit.Id, model.CommitWebUri));

            Footers = footers;
        }


        private string? GetScopeDisplayName()
        {
            if (String.IsNullOrEmpty(m_Model.Scope))
                return null;

            var displayName = m_Configuration.Scopes.FirstOrDefault(scope => StringComparer.OrdinalIgnoreCase.Equals(scope.Name, m_Model.Scope))?.DisplayName;

            return !String.IsNullOrWhiteSpace(displayName) ? displayName : m_Model.Scope;
        }
    }
}
