using System;

namespace Grynwald.ChangeLog.Model.Text
{
    public interface IWebLinkTextElement : ITextElement
    {
        Uri Uri { get; }
    }
}
