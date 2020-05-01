using System;
using System.Collections.Generic;
using System.Linq;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Model;

namespace Grynwald.ChangeLog.Templates.ViewModel
{
    /// <summary>
    /// Represents the view of a single change log entry
    /// </summary>
    internal class ChangeLogEntryViewModel
    {
        private readonly ChangeLogConfiguration m_Configuration;
        private readonly ChangeLogEntry m_Model;

        /// <summary>
        /// Gets the title of the change log entry.
        /// This includes both the summary of the entry and the scope (if a scope was specified)
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the change log entry's date
        /// </summary>
        public DateTime Date => m_Model.Date;

        /// <summary>
        /// Gets the message body for the change log entry
        /// </summary>
        public IReadOnlyList<string> Body => m_Model.Body;

        /// <summary>
        /// Gets the change log entry's type specified in the commit message header(e.g. <c>feat</c> of <c>fix</c>)
        /// </summary>
        public CommitType Type => m_Model.Type;

        /// <summary>
        /// Gets the commit the change
        /// </summary>
        public GitId Commit => m_Model.Commit;

        /// <summary>
        /// Gets all the footers for the change log entry **excluding** "BREAKING CHANGES" footers.
        /// To retrieve breaking changes, use <see cref="BreakingChanges"/>
        /// </summary>
        public IReadOnlyList<ChangeLogEntryFooterViewModel> Footers { get; }

        /// <summary>
        /// Gets the descriptions of breaking changes for this change log entry.
        /// </summary>
        /// <remarks>
        /// This property returns all descriptions for breaking changes provided as a "BREAKING CHANGE" footer
        /// or a single description identical to <see cref="Title"/>, if the change log entry was
        /// marked as breaking change in the header (using '!')
        /// </remarks>
        public IReadOnlyList<BreakingChangeViewModel> BreakingChanges { get; }


        /// <summary>
        /// Initializes a new instance of <see cref="ChangeLogEntryViewModel"/>
        /// </summary>
        public ChangeLogEntryViewModel(ChangeLogConfiguration configuration, ChangeLogEntry model)
        {
            m_Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            m_Model = model ?? throw new ArgumentNullException(nameof(model));

            Title = GetTitle();
            Footers = LoadFooters();
            BreakingChanges = LoadBreakingChanges();
        }


        private IReadOnlyList<ChangeLogEntryFooterViewModel> LoadFooters()
        {
            // Load all footers except "BREAKING CHANGE" footers. They are handled separately
            var footers = m_Model.Footers
                .Where(x => x.Name != CommitMessageFooterName.BreakingChange)
                .Select(x => new ChangeLogEntryFooterViewModel(m_Configuration, x))
                .ToList();

            // The commit id (and web link) is included in the changelog as a implicit footer
            footers.Add(new ChangeLogEntryFooterViewModel(m_Configuration, "Commit", m_Model.Commit.Id, m_Model.CommitWebUri));

            return footers;
        }

        private IReadOnlyList<BreakingChangeViewModel> LoadBreakingChanges()
        {
            if (m_Model.BreakingChangeDescriptions.Any())
            {
                return m_Model.BreakingChangeDescriptions
                    .Select(x => new BreakingChangeViewModel(m_Configuration, x, this, false))
                    .ToList();
            }
            else if (m_Model.ContainsBreakingChanges)
            {
                return new[] { new BreakingChangeViewModel(m_Configuration, GetTitle(), this, true) };
            }
            else
            {
                return Array.Empty<BreakingChangeViewModel>();
            }
        }

        private string GetTitle()
        {
            return String.IsNullOrEmpty(m_Model.Scope) ? m_Model.Summary : $"{GetScopeDisplayName()}: {m_Model.Summary}";
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
