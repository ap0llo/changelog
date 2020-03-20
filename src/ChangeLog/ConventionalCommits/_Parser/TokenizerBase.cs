using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Grynwald.ChangeLog.ConventionalCommits
{
    public abstract class TokenizerBase<T> where T : class
    {
        public IEnumerable<T> GetTokens(LineToken input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (input.Kind != LineTokenKind.Line)
                throw new ArgumentException($"Input must be line token of kind '{LineTokenKind.Line}', but is kind '{input.Kind}'");


            var currentValue = new StringBuilder();

            var startColumn = 1;

            if (input.Value!.Length == 0)
            {
                yield return CreateEolToken(input.LineNumber, startColumn);
                yield break;
            }

            for (var i = 0; i < input.Value.Length; i++)
            {
                var currentChar = input.Value[i];
                if (TryMatchSingleCharToken(currentChar, input.LineNumber, i + 1, out var matchedToken))
                {
                    if (currentValue.Length > 0)
                    {
                        var value = currentValue.GetValueAndClear();
                        yield return CreateStringToken(value, input.LineNumber, startColumn);
                        startColumn += value.Length;
                    }
                    yield return matchedToken;
                    startColumn += 1;
                }
                else
                {
                    currentValue.Append(currentChar);
                }

            }

            // if any input is left in currentValue, return it as String token
            if (currentValue.Length > 0)
            {
                var value = currentValue.GetValueAndClear();
                yield return CreateStringToken(value, input.LineNumber, startColumn);
                startColumn += value.Length;
            }

            yield return CreateEolToken(input.LineNumber, startColumn);
        }

        protected abstract bool TryMatchSingleCharToken(char value, int lineNumber, int columnNumber, [NotNullWhen(true)] out T? token);

        protected abstract T CreateEolToken(int lineNumber, int columnNumber);

        protected abstract T CreateStringToken(string value, int lineNumber, int columnNumber);
    }
}
