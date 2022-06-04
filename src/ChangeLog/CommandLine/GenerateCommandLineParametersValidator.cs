using System.IO;
using FluentValidation;
using Grynwald.ChangeLog.Validation;

namespace Grynwald.ChangeLog.CommandLine
{
    /// <summary>
    /// Validator for <see cref="GenerateCommandLineParameters"/>
    /// </summary>
    internal class GenerateCommandLineParametersValidator : AbstractValidator<GenerateCommandLineParameters>
    {
        public GenerateCommandLineParametersValidator()
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
