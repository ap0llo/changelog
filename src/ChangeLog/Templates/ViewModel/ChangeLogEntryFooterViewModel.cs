using System;
using System.Linq;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.ChangeLog.Model;

namespace Grynwald.ChangeLog.Templates.ViewModel
{
    internal class ChangeLogEntryFooterViewModel
    {
        private readonly ChangeLogConfiguration m_Configuration;


        public string DisplayName { get; }

        public string Value { get; }

        public Uri? WebUri { get; }


        public ChangeLogEntryFooterViewModel(ChangeLogConfiguration configuration, string name, string value, Uri? webUri)
        {
            m_Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value must not be null or whitespace", nameof(name));

            if (String.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value must not be null or whitespace", nameof(value));

            DisplayName = GetFooterDisplayName(name);
            Value = value;
            WebUri = webUri;
        }

        public ChangeLogEntryFooterViewModel(ChangeLogConfiguration configuration, ChangeLogEntryFooter model)
        {
            m_Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            if (model is null)
                throw new ArgumentNullException(nameof(model));

            DisplayName = GetFooterDisplayName(model.Name.Value);
            Value = model.Value;
            WebUri = model.WebUri;

        }

        private string GetFooterDisplayName(string name)
        {
            var footerName = new CommitMessageFooterName(name);

            var displayName = m_Configuration.Footers
                .FirstOrDefault(c =>
                    !String.IsNullOrWhiteSpace(c.Name) &&
                    new CommitMessageFooterName(c.Name).Equals(footerName)
                )?.DisplayName;

            return !String.IsNullOrWhiteSpace(displayName) ? displayName : name;
        }
    }
}
