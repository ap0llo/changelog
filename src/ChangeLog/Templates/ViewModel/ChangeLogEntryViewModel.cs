using System;
using Grynwald.ChangeLog.Model;

namespace Grynwald.ChangeLog.Templates.ViewModel
{
    internal class ChangeLogEntryViewModel
    {
        public string Title { get; }

        public DateTime Date { get; }


        public ChangeLogEntryViewModel(ChangeLogEntry model)
        {
            if (model is null)
                throw new ArgumentNullException(nameof(model));

            //TODO: Scope display names
            Title = String.IsNullOrEmpty(model.Scope) ? model.Summary : $"{model.Scope}: {model.Summary}";
            Date = model.Date;
        }
    }
}
