using System;

namespace Grynwald.ChangeLog.Model.Text
{
    public sealed class PlainTextElement : ITextElement
    {
        /// <inheritdoc />
        public string Text { get; }


        public PlainTextElement(string text)
        {
            if (String.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Value must not be null or whitespace", nameof(text));

            Text = text;
        }
    }
}
