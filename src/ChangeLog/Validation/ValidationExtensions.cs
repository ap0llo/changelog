using System;
using System.Reflection;
using CommandLine;
using FluentValidation;
using Grynwald.ChangeLog.Configuration;
using NuGet.Versioning;

namespace Grynwald.ChangeLog.Validation
{
    /// <summary>
    /// Provides additional validation functionality for validators based on FluentValidation
    /// </summary>
    internal static class ValidationExtensions
    {
        /// <summary>
        /// Sets <see cref="ValidatorConfiguration.DisplayNameResolver"/> to a custom resolver that handles <see cref="SettingDisplayNameAttribute"/> and <see cref="OptionAttribute"/>.
        /// </summary>
        public static void UseCustomDisplayNameResolver(this ValidatorConfiguration validatorConfiguration)
        {
            validatorConfiguration.DisplayNameResolver = (type, member, expression) =>
            {
                if (member is PropertyInfo property)
                {
                    // If the property has a [SettingDisplayName] attribute, use value defined here
                    var displayName = property?.GetCustomAttribute<SettingDisplayNameAttribute>()?.DisplayName;
                    if (!String.IsNullOrEmpty(displayName))
                    {
                        return displayName;
                    }

                    // If the value has a [Option] attribute (used by the command line parser)
                    displayName = property?.GetCustomAttribute<OptionAttribute>()?.LongName;
                    if (!String.IsNullOrEmpty(displayName))
                    {
                        return displayName;
                    }
                }

                // default case: No display name
                return null;
            };
        }



        /// <summary>
        /// Defines a  validator that will fail if the property is NOT <c>null</c> or empty BUT consists only of whitespace characters.
        /// </summary>
        public static IRuleBuilderOptions<T, string?> NotWhitespace<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder
                .Must(x => !String.IsNullOrWhiteSpace(x))
                .WithMessage("'{PropertyName}' must not be whitespace")
                .UnlessNullOrEmpty();
        }

        /// <summary>
        /// Disables the rule if the value is <c>null</c> or empty.
        /// </summary>
        public static IRuleBuilderOptions<T, string?> UnlessNullOrEmpty<T>(this IRuleBuilderOptions<T, string?> rule)
        {
            return rule.Configure(config =>
            {
                config.ApplyCondition(context => !String.IsNullOrEmpty(config.GetPropertyValue(context.InstanceToValidate) as string));
            });
        }

        /// <summary>
        /// Disables the rule if the value is <c>null</c> or whitespace.
        /// </summary>
        public static IRuleBuilderOptions<T, string?> UnlessNullOrWhiteSpace<T>(this IRuleBuilderOptions<T, string?> rule)
        {
            return rule.Configure(config =>
            {
                config.ApplyCondition(context => !String.IsNullOrWhiteSpace(config.GetPropertyValue(context.InstanceToValidate) as string));
            });
        }

        /// <summary>
        /// Defines a validator that will fail if the value cannot be parsed as <see cref="VersionRange"/>.
        /// </summary>
        public static IRuleBuilderOptions<T, string?> IsVersionRange<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder
                .Must(x => x is not null && VersionRange.TryParse(x, out _))
                .WithMessage("Value '{PropertyValue}' of '{PropertyName}' is not a valid version range.");
        }

        /// <summary>
        /// Defines a validator that will fail if the value cannot be parsed as <see cref="NuGetVersion"/>.
        /// </summary>
        public static IRuleBuilderOptions<T, string?> IsNuGetVersion<T>(this IRuleBuilder<T, string?> ruleBuilder)
        {
            return ruleBuilder
                .Must(x => NuGetVersion.TryParse(x, out _))
                .WithMessage("Value '{PropertyValue}' of '{PropertyName}' is not a valid version."); ;
        }
    }
}
