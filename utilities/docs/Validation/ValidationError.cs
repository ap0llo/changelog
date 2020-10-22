using System;

namespace docs.Validation
{
    public class ValidationError
    {
        public string RuleId { get; }

        public string Message { get; }

        public int LineNumber { get; }


        public ValidationError(string ruleId, string message, int lineNumber)
        {
            if (String.IsNullOrWhiteSpace(ruleId))
                throw new ArgumentException($"'{nameof(ruleId)}' cannot be null or whitespace", nameof(ruleId));

            if (String.IsNullOrWhiteSpace(message))
                throw new ArgumentException($"'{nameof(message)}' cannot be null or whitespace", nameof(message));

            RuleId = ruleId;
            Message = message;
            LineNumber = lineNumber;
        }
    }
}
