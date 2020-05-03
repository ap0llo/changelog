using System;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Model;
using Microsoft.Extensions.Logging;

namespace Grynwald.ChangeLog.Configuration
{
    internal class ConfigurationValidator
    {
        private readonly ILogger<ConfigurationValidator> m_Logger;

        public ConfigurationValidator(ILogger<ConfigurationValidator> logger)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public bool Validate(ChangeLogConfiguration configuration)
        {
            if (configuration is null)
                throw new ArgumentNullException(nameof(configuration));


            return true;
        }

    }
}
