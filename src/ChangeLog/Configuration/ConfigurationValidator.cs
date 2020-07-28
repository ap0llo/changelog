using System;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;

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

            var valid = true;

            // Scopes name must not be null or whitespace
            foreach (var scopeConfig in configuration.Scopes)
            {
                if (String.IsNullOrWhiteSpace(scopeConfig.Name))
                {
                    m_Logger.LogError("Invalid configuration: Scope name must not be null or whitespace.");
                    valid = false;
                }
            }

            // Footer name must not be null or whitespace
            foreach (var footerConfiguration in configuration.Footers)
            {
                if (String.IsNullOrWhiteSpace(footerConfiguration.Name))
                {
                    m_Logger.LogError("Invalid configuration: Footer name must not be null or whitespace.");
                    valid = false;
                }
            }

            // VersionRange must be valid (if set)
            if (!String.IsNullOrEmpty(configuration.VersionRange))
            {
                if (String.IsNullOrWhiteSpace(configuration.VersionRange))
                {
                    m_Logger.LogError("Invalid configuration: Value of setting 'Version Range' must not be whitespace.");
                    valid = false;
                }
                else if (!VersionRange.TryParse(configuration.VersionRange, out _))
                {
                    m_Logger.LogError($"Invalid configuration: Value '{configuration.VersionRange}' of setting 'Version Range' is not a valid version range.");
                    valid = false;
                }
            }

            // CurrentVersion must be valid (if set)
            if (!String.IsNullOrEmpty(configuration.CurrentVersion))
            {
                if (String.IsNullOrWhiteSpace(configuration.CurrentVersion))
                {
                    m_Logger.LogError("Invalid configuration: Value of setting 'Current Version' must not be whitespace.");
                    valid = false;
                }
                else if (!VersionRange.TryParse(configuration.CurrentVersion, out _))
                {
                    m_Logger.LogError($"Invalid configuration: Value '{configuration.CurrentVersion}' of setting 'Current Version' is not a valid version.");
                    valid = false;
                }
            }

            // Entry type name must not be null or whitespace
            foreach (var entryTypeConfig in configuration.EntryTypes)
            {
                if (String.IsNullOrWhiteSpace(entryTypeConfig.Type))
                {
                    m_Logger.LogError("Invalid configuration: Entry type must not be null or whitespace.");
                    valid = false;
                }
            }

            // GitHub Access token must not be whitespace (if set)
            if (!String.IsNullOrEmpty(configuration.Integrations.GitHub.AccessToken))
            {
                if (String.IsNullOrWhiteSpace(configuration.Integrations.GitHub.AccessToken))
                {
                    m_Logger.LogError("Invalid configuration: GitHub access token must not be whitespace");
                    valid = false;
                }
            }

            // GitHub remote name must not be null or whitespace
            if (String.IsNullOrWhiteSpace(configuration.Integrations.GitHub.RemoteName))
            {
                m_Logger.LogError("Invalid configuration: GitHub remote name must not be null or whitespace");
                valid = false;
            }

            // GitHub host must not be whitespace (if set)
            if (!String.IsNullOrEmpty(configuration.Integrations.GitHub.Host))
            {
                if (String.IsNullOrWhiteSpace(configuration.Integrations.GitHub.Host))
                {
                    m_Logger.LogError("Invalid configuration: GitHub host must not be whitespace");
                    valid = false;
                }
            }

            // GitHub owner must not be whitespace (if set)
            if (!String.IsNullOrEmpty(configuration.Integrations.GitHub.Owner))
            {
                if (String.IsNullOrWhiteSpace(configuration.Integrations.GitHub.Owner))
                {
                    m_Logger.LogError("Invalid configuration: GitHub owner not be whitespace");
                    valid = false;
                }
            }

            // GitHub repository must not be whitespace (if set)
            if (!String.IsNullOrEmpty(configuration.Integrations.GitHub.Repository))
            {
                if (String.IsNullOrWhiteSpace(configuration.Integrations.GitHub.Repository))
                {
                    m_Logger.LogError("Invalid configuration: GitHub repository not be whitespace");
                    valid = false;
                }
            }

            // GitLab Access token must not be whitespace (if set)
            if (!String.IsNullOrEmpty(configuration.Integrations.GitLab.AccessToken))
            {
                if (String.IsNullOrWhiteSpace(configuration.Integrations.GitLab.AccessToken))
                {
                    m_Logger.LogError("Invalid configuration: GitLab access token must not be whitespace");
                    valid = false;
                }
            }

            return valid;
        }
    }
}
