using System;
using System.Collections.Generic;
using docs.Validation;

namespace docs
{
    internal static class DocsValidator
    {
        private static readonly IReadOnlyList<IRule> s_Rules = new IRule[]
        {
            new TemplateOutputIsUpToDateRule(),
            new RealtiveLinksAreValidRule()
        };


        public static ValidationResult ValidateDocument(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
                throw new ArgumentException($"'{nameof(path)}' cannot be null or whitespace", nameof(path));

            var result = new ValidationResult();
            foreach (var rule in s_Rules)
            {
                rule.Apply(path, result);
            }

            return result;
        }
    }
}
