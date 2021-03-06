﻿using System;
using System.Linq;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.ChangeLog.Model;

namespace Grynwald.ChangeLog.Templates
{

    internal static class ModelExtensions
    {
        /// <summary>
        /// Gets the display name for the entry's scope.
        /// </summary>
        /// <returns>
        /// Returns the display name for the entry's scope as configured in <paramref name="configuration"/>.
        /// If the entry has no scope, returns null.
        /// If no display name is configured for the scope, returns the scope itself.
        /// </returns>
        public static string? GetScopeDisplayName(this ChangeLogEntry entry, ChangeLogConfiguration configuration)
        {
            if (String.IsNullOrEmpty(entry.Scope))
                return null;

            var displayName = configuration.Scopes.FirstOrDefault(kvp => StringComparer.OrdinalIgnoreCase.Equals(kvp.Key, entry.Scope)).Value?.DisplayName;

            return !String.IsNullOrWhiteSpace(displayName) ? displayName : entry.Scope;
        }

        /// <summary>
        /// Gets the display name for the footer's name.
        /// </summary>
        /// <returns>
        /// Returns the display name for the footer's name as configured in <paramref name="configuration"/>.
        /// If no display name is configured for the scope, returns the footer name.
        /// </returns>
        public static string GetFooterDisplayName(this ChangeLogEntryFooter footer, ChangeLogConfiguration configuration)
        {
            var displayName = configuration.Footers
                .FirstOrDefault(kvp =>
                    !String.IsNullOrWhiteSpace(kvp.Key) &&
                    new CommitMessageFooterName(kvp.Key).Equals(footer.Name)
                ).Value?.DisplayName;

            return !String.IsNullOrWhiteSpace(displayName) ? displayName : footer.Name.Value;
        }
    }
}
