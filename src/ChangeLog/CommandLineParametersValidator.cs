using System;
using System.IO;
using System.Reflection;
using CommandLine;
using FluentValidation;
using NuGet.Versioning;

namespace Grynwald.ChangeLog
{
    /// <summary>
    /// Validator for <see cref="CommandLineParameters"/>
    /// </summary>
    internal class CommandLineParametersValidator : AbstractValidator<CommandLineParameters>
    {
        public CommandLineParametersValidator()
        {
            ValidatorOptions.DisplayNameResolver = (type, member, expression) =>
            {
                return (member is PropertyInfo property)
                    ? property?.GetCustomAttribute<OptionAttribute>()?.LongName
                    : null;
            };

            RuleFor(x => x.RepositoryPath).NotEmpty();
            RuleFor(x => x.RepositoryPath)
                .Must(Directory.Exists)
                .When(x => !String.IsNullOrWhiteSpace(x.RepositoryPath))
                .WithMessage("Directory '{PropertyValue}' does not exists (parameter {PropertyName})");

            RuleFor(x => x.ConfigurationFilePath)
                .Must(File.Exists)
                .When(x => !String.IsNullOrEmpty(x.ConfigurationFilePath))
                .WithMessage("File '{PropertyValue}' does not exists (parameter {PropertyName})");

            RuleFor(x => x.CurrentVersion)
                .Must(val => NuGetVersion.TryParse(val, out _))
                .When(x => !String.IsNullOrEmpty(x.CurrentVersion))
                .WithMessage("Value '{PropertyValue}' of parameter '{PropertyName}' is not a valid version.");

            RuleFor(x => x.VersionRange)
                .Must(val => VersionRange.TryParse(val, out _))
                .When(x => !String.IsNullOrEmpty(x.VersionRange))
                .WithMessage("Value '{PropertyValue}' of parameter '{PropertyName}' is not a valid version range.");
        }

    }
}
