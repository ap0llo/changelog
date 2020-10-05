using System;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Grynwald.ChangeLog.ConventionalCommits;
using Grynwald.ChangeLog.Validation;

namespace Grynwald.ChangeLog.Configuration
{
    /// <summary>
    /// Validator for <see cref="ChangeLogConfiguration"/>
    /// </summary>
    internal class ConfigurationValidator : AbstractValidator<ChangeLogConfiguration>
    {
        public ConfigurationValidator()
        {
            ValidatorOptions.Global.UseCustomDisplayNameResolver();

            RuleForEach(x => x.Scopes)
                .ChildRules(scope => scope.RuleFor(x => x.Key).NotEmpty().WithName("Scope Name"))
                .DependentRules(() =>
                    RuleFor(x => x.Scopes.Keys)
                       .Must(keys => keys.Distinct(StringComparer.OrdinalIgnoreCase).Count() == keys.Count)
                       .WithMessage("'Scope Name' must be unique"));

            RuleForEach(x => x.Footers)
                .ChildRules(footer => footer.RuleFor(x => x.Key).NotEmpty().WithName("Footer Name"))
                .DependentRules(() =>
                    RuleFor(x => x.Footers.Keys)
                       .Must(keys =>
                       {
                           var footerNames = keys.Select(key => new CommitMessageFooterName(key));
                           return footerNames.Distinct().Count() == keys.Count;
                       })
                       .WithMessage("'Footer Name' must be unique"));

            RuleForEach(x => x.EntryTypes)
                .ChildRules(entryType => entryType.RuleFor(x => x.Type).NotEmpty());

            RuleFor(x => x.VersionRange).NotWhitespace();
            RuleFor(x => x.VersionRange).IsVersionRange().UnlessNullOrEmpty();

            RuleFor(x => x.CurrentVersion).NotWhitespace();
            RuleFor(x => x.CurrentVersion).IsNuGetVersion().UnlessNullOrEmpty();

            RuleFor(x => x.Integrations.GitHub.AccessToken).NotWhitespace();
            RuleFor(x => x.Integrations.GitHub.RemoteName).NotEmpty();
            RuleFor(x => x.Integrations.GitHub.Host).NotWhitespace();
            RuleFor(x => x.Integrations.GitHub.Owner).NotWhitespace();
            RuleFor(x => x.Integrations.GitHub.Repository).NotWhitespace();

            RuleFor(x => x.Integrations.GitLab.AccessToken).NotWhitespace();
            RuleFor(x => x.Integrations.GitLab.RemoteName).NotEmpty();
            RuleFor(x => x.Integrations.GitLab.Host).NotWhitespace();
            RuleFor(x => x.Integrations.GitLab.Namespace).NotWhitespace();
            RuleFor(x => x.Integrations.GitLab.Project).NotWhitespace();

            RuleForEach(x => x.Filter.Include)
                .ChildRules(filter => filter.RuleFor(f => f.Type).NotWhitespace())
                .ChildRules(filter => filter.RuleFor(f => f.Scope).NotWhitespace());

            RuleForEach(x => x.Filter.Exclude)
                .ChildRules(filter => filter.RuleFor(f => f.Type).NotWhitespace())
                .ChildRules(filter => filter.RuleFor(f => f.Scope).NotWhitespace());
        }


        public override ValidationResult Validate(ValidationContext<ChangeLogConfiguration> context)
        {
            if (context.InstanceToValidate is null)
                throw new ArgumentNullException("instance");

            return base.Validate(context);
        }
    }
}
