using System;

namespace Grynwald.ChangeLog.Model
{
    /// <summary>
    /// Represents a hyperlink to a url
    /// </summary>
    public sealed class WebLink : ILink
    {
        /// <summary>
        /// The link's uri.
        /// </summary>
        public Uri Uri { get; }


        /// <summary>
        /// Initializes a new instance of <see cref="WebLink"/>
        /// </summary>
        /// <param name="uri"></param>
        public WebLink(Uri uri)
        {
            Uri = uri ?? throw new ArgumentNullException(nameof(uri));
        }
    }
}
