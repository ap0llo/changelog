namespace Grynwald.ChangeLog.Model.Text
{
    interface INormalizedTextElement : ITextElement
    {
        public string NormalizedText { get; }

        public TextStyle NormalizedStyle { get; }
    }
}
