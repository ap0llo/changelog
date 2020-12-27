using System;

namespace Grynwald.ChangeLog.Model.Text
{
    public class WebLinkTextElement : IWebLinkTextElement
    {
        /// <inheritdoc />
        public string Text { get; }

        /// <inheritdoc />  
        public TextStyle Style => TextStyle.None;

        /// <summary>
        /// The link's uri.
        /// </summary>
        public Uri Uri { get; }


        public WebLinkTextElement(string text, Uri uri)
        {
            if (String.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Value must not be null or whitespace", nameof(text));

            Uri = uri ?? throw new ArgumentNullException(nameof(uri));
            Text = text;
        }
    }
}
