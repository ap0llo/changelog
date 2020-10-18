using System;

namespace Grynwald.ChangeLog.Model.Text
{
    public abstract class TextElement
    {

        public string Text { get; }


        public TextElement(string text)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }
    }
}
