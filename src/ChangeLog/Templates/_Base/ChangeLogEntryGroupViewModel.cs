using System;
using System.Collections.Generic;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.ChangeLog.Model;

namespace Grynwald.ChangeLog.Templates
{
    internal class ChangeLogEntryGroupViewModel
    {
        private readonly ChangeLogEntryGroup m_Model;


        public string DisplayName => GetCommitTypeDisplayName(m_Model.Type);

        public IEnumerable<ChangeLogEntry> Entries => m_Model.Entries;


        public ChangeLogEntryGroupViewModel(ChangeLogEntryGroup model)
        {
            m_Model = model ?? throw new ArgumentNullException(nameof(model));
        }


        protected string GetCommitTypeDisplayName(CommitType type)
        {
            if (type == CommitType.Feature)
            {
                return "New Features";
            }
            else if (type == CommitType.BugFix)
            {
                return "Bug Fixes";
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }

}
