using System;
using Grynwald.ChangeLog.Git;

namespace Grynwald.ChangeLog.Model.Text
{
    class CommitReferenceTextElementWithWebLink : CommitReferenceTextElement, IWebLinkTextElement
    {
        /// <inheritdoc />
        public Uri Uri { get; }

        public CommitReferenceTextElementWithWebLink(string text, GitId id, Uri uri) : base(text, id)
        {
            Uri = uri ?? throw new ArgumentNullException(nameof(uri));
        }


        public static CommitReferenceTextElementWithWebLink FromCommitReference(CommitReferenceTextElement commitReference, Uri uri) =>
            new CommitReferenceTextElementWithWebLink(commitReference.Text, commitReference.CommitId, uri);
    }
}
