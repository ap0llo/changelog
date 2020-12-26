using System.IO;
using FluentValidation;
using Grynwald.ChangeLog.Validation;

namespace Grynwald.ChangeLog
{
    /// <summary>
    /// Validator for <see cref="CommandLineParameters"/>
    /// </summary>
    internal class CommandLineParametersValidator : AbstractValidator<CommandLineParameters>
    {
        public CommandLineParametersValidator()
        {
            ValidatorOptions.Global.UseCustomDisplayNameResolver();

            RuleFor(x => x.RepositoryPath).NotWhitespace();
            RuleFor(x => x.RepositoryPath)
                .Must(Directory.Exists)
                .UnlessNullOrWhiteSpace()
                .WithMessage("Directory '{PropertyValue}' does not exists (parameter '{PropertyName}')");

            RuleFor(x => x.ConfigurationFilePath)
                .Must(File.Exists)
                .UnlessNullOrEmpty()
                .WithMessage("File '{PropertyValue}' does not exists (parameter '{PropertyName}')");

            RuleFor(x => x.CurrentVersion)
                .IsNuGetVersion()
                .UnlessNullOrEmpty();

            RuleFor(x => x.VersionRange)
                .IsVersionRange()
                .UnlessNullOrEmpty();
        }

    }
}
