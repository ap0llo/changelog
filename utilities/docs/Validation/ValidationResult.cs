using System.Collections.Generic;
using System.Linq;

namespace docs.Validation
{
    public class ValidationResult
    {
        private readonly List<ValidationError> m_Errors = new List<ValidationError>();

        public bool Success => !Errors.Any();

        public IReadOnlyList<ValidationError> Errors => m_Errors;


        public void AddError(string ruleId, string error, int lineNumber = 0)
        {
            m_Errors.Add(new ValidationError(ruleId, error, lineNumber));
        }
    }
}
