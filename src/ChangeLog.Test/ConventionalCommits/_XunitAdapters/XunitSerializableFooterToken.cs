﻿using System;
using Grynwald.ChangeLog.ConventionalCommits;
using Xunit.Abstractions;

namespace Grynwald.ChangeLog.Test.ConventionalCommits
{
    /// <summary>
    /// Wrapper class to make <see cref="FooterToken"/> serializable by xunit
    /// </summary>
    public class XunitSerializableFooterToken : IXunitSerializable
    {
        internal FooterToken Value { get; private set; }


        internal XunitSerializableFooterToken(FooterToken value) => Value = value;


        [Obsolete("For use by Xunit only", true)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public XunitSerializableFooterToken()
        { }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        public void Deserialize(IXunitSerializationInfo info)
        {
            var kind = info.GetValue<FooterTokenKind>(nameof(HeaderToken.Kind));
            var value = info.GetValue<string>(nameof(HeaderToken.Value));
            var lineNumber = info.GetValue<int>(nameof(HeaderToken.LineNumber));
            var columnNumber = info.GetValue<int>(nameof(HeaderToken.ColumnNumber));

            Value = new FooterToken(kind, value, lineNumber, columnNumber);
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(Value.Kind), Value.Kind);
            info.AddValue(nameof(Value.Value), Value.Value);
            info.AddValue(nameof(Value.LineNumber), Value.LineNumber);
            info.AddValue(nameof(Value.ColumnNumber), Value.ColumnNumber);
        }

        public override string? ToString() => Value?.ToString();
    }
}
