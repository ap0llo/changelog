using System;

namespace Grynwald.ChangeLog.Model.Text
{
    public class WebLinkTextElement : TextElement, IWebLinkTextElement
    {
        /// <summary>
        /// The link's uri.
        /// </summary>
        public Uri Uri { get; }


        public WebLinkTextElement(string text, Uri uri) : base(text)
        {
            Uri = uri ?? throw new ArgumentNullException(nameof(uri));
        }
    }
}
