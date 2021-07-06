using System;
using Grynwald.ChangeLog.Model;

namespace Grynwald.ChangeLog.Pipeline
{
    public sealed class ChangeLogPipelineResult
    {
        private readonly ApplicationChangeLog? m_ChangeLog;


        public bool Success { get; }

        public ApplicationChangeLog Value
        {
            get => Success ? m_ChangeLog! : throw new InvalidOperationException("Cannot access value of a error result");
        }


        private ChangeLogPipelineResult(bool success, ApplicationChangeLog? changelog)
        {
            Success = success;
            m_ChangeLog = changelog;
        }


        public static ChangeLogPipelineResult CreateErrorResult() => new ChangeLogPipelineResult(false, null);

        public static ChangeLogPipelineResult CreateSuccessResult(ApplicationChangeLog value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            return new ChangeLogPipelineResult(true, value);
        }
    }
}
