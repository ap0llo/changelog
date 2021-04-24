using System;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Model.Text;

namespace Grynwald.ChangeLog.Templates.ViewModel
{
    internal class ChangeLogEntryFooterViewModel
    {
        private readonly ChangeLogConfiguration m_Configuration;
        private readonly ChangeLogEntryFooter m_Model;


        // TODO: Remove public property once all templates only access viewmodel properties
        public ChangeLogEntryFooter Model => m_Model;


        public string Name => m_Model.Name.Value;

        public string DisplayName => m_Model.GetFooterDisplayName(m_Configuration);

        public TextStyle Style => m_Model.Value.Style;

        public string Value => m_Model.Value.Text;

        public string NormalizedValue => (m_Model.Value is INormalizedTextElement normalizedText) ? normalizedText.NormalizedText : Value;

        public TextStyle NormalizedStyle => (m_Model.Value is INormalizedTextElement normalizedText) ? normalizedText.NormalizedStyle : Style;


        public ChangeLogEntryFooterViewModel(ChangeLogConfiguration configuration, ChangeLogEntryFooter model)
        {
            m_Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            m_Model = model ?? throw new ArgumentNullException(nameof(model));
        }

    }
}
