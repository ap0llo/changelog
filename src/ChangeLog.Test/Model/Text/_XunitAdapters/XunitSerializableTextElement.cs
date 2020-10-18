using System;
using Grynwald.ChangeLog.Git;
using Grynwald.ChangeLog.Model.Text;
using Xunit.Abstractions;

namespace Grynwald.ChangeLog.Test.Model.Text
{
    /// <summary>
    /// Wrapper class to make instances of <see cref="TextElement"/> serializable by xunit
    /// </summary>
    internal sealed class XunitSerializableTextElement : IXunitSerializable
    {
        private const string s_Type = "Type";

        internal TextElement Value { get; private set; }


        internal XunitSerializableTextElement(TextElement value) => Value = value;


        [Obsolete("For use by Xunit only", true)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public XunitSerializableTextElement()
        { }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        public void Deserialize(IXunitSerializationInfo info)
        {
            var type = info.GetValue<string>(s_Type);
            switch (type)
            {
                case nameof(PlainTextElement):
                    Value = new PlainTextElement(info.GetValue<string>(nameof(PlainTextElement.Text)));
                    break;

                case nameof(WebLinkTextElement):
                    Value = new WebLinkTextElement(
                        info.GetValue<string>(nameof(WebLinkTextElement.Text)),
                        new Uri(info.GetValue<string>(nameof(WebLinkTextElement.Uri)))
                    );
                    break;

                case nameof(CommitReferenceTextElement):
                    var id = info.GetValue<string>(nameof(CommitReferenceTextElement.CommitId.Id));
                    var abbreviatedId = info.GetValue<string>(nameof(CommitReferenceTextElement.CommitId.AbbreviatedId));
                    Value = new CommitReferenceTextElement(
                        info.GetValue<string>(nameof(CommitReferenceTextElement.Text)),
                        new GitId(id, abbreviatedId)
                    );
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            switch (Value)
            {
                case PlainTextElement plainText:
                    info.AddValue(s_Type, nameof(PlainTextElement));
                    info.AddValue(nameof(PlainTextElement.Text), plainText.Text);
                    break;

                case WebLinkTextElement webLink:
                    info.AddValue(s_Type, nameof(WebLinkTextElement));
                    info.AddValue(nameof(WebLinkTextElement.Text), webLink.Text);
                    info.AddValue(nameof(WebLinkTextElement.Uri), webLink.Uri.ToString());
                    break;

                case CommitReferenceTextElement commitReference:
                    info.AddValue(s_Type, nameof(CommitReferenceTextElement));
                    info.AddValue(nameof(CommitReferenceTextElement.CommitId.Id), commitReference.CommitId.Id);
                    info.AddValue(nameof(CommitReferenceTextElement.CommitId.AbbreviatedId), commitReference.CommitId.AbbreviatedId);
                    break;

                default:
                    throw new NotImplementedException();
            }

        }
    }
}
