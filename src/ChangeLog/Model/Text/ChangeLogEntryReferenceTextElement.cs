using System;

namespace Grynwald.ChangeLog.Model.Text
{
    public sealed class ChangeLogEntryReferenceTextElement : TextElement
    {
        public ChangeLogEntry Entry { get; }

        public ChangeLogEntryReferenceTextElement(string text, ChangeLogEntry entry) : base(text)
        {
            Entry = entry ?? throw new ArgumentNullException(nameof(entry));
        }
    }
}
