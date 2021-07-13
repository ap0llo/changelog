using System;

namespace Grynwald.ChangeLog.Pipeline
{
    internal class InvalidPipelineConfigurationException : Exception
    {
        public InvalidPipelineConfigurationException(string? message) : base(message)
        {
        }
    }
}
