using System;
using Grynwald.ChangeLog.Model;
using Xunit.Abstractions;

namespace Grynwald.ChangeLog.Test.Model
{
    /// <summary>
    /// Wrapper class to make instances of <see cref="ILink"/> serializable by xunit
    /// </summary>
    internal sealed class XunitSerializableLink : IXunitSerializable
    {
        private const string s_Type = "Type";

        internal ILink Value { get; private set; }


        internal XunitSerializableLink(ILink value) => Value = value;


        [Obsolete("For use by Xunit only", true)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public XunitSerializableLink()
        { }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        public void Deserialize(IXunitSerializationInfo info)
        {
            var type = info.GetValue<string>(s_Type);
            switch (type)
            {
                case nameof(WebLink):
                    var uri = new Uri(info.GetValue<string>(nameof(WebLink.Uri)));
                    Value = new WebLink(uri);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            switch (Value)
            {
                case WebLink webLink:
                    info.AddValue(s_Type, nameof(WebLink));
                    info.AddValue(nameof(WebLink.Uri), webLink.Uri.ToString());
                    break;

                default:
                    throw new NotImplementedException();
            }

        }
    }
}
