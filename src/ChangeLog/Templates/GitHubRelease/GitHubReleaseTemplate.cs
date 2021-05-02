using System.Linq;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Templates.ViewModel;
using Grynwald.MarkdownGenerator;

namespace Grynwald.ChangeLog.Templates.GitHubRelease
{
    internal class GitHubReleaseTemplate : MarkdownBaseTemplate
    {
        /// <inheritdoc />
        protected override MdSerializationOptions SerializationOptions { get; }

        /// <inheritdoc />
        protected override bool EnableNormalization => m_Configuration.Template.GitHubRelease.NormalizeReferences;


        public GitHubReleaseTemplate(ChangeLogConfiguration configuration) : base(configuration)
        {
            SerializationOptions = MdSerializationOptions.Presets.Default
                .With(opts => { opts.HeadingAnchorStyle = MdHeadingAnchorStyle.Auto; });
        }


        /// <inheritdoc />
        protected override MdDocument GetChangeLogDocument(ApplicationChangeLogViewModel viewModel)
        {
            if (viewModel.ChangeLogs.Count() > 1)
                throw new TemplateExecutionException("The GitHub Release template cannot render change logs that contain multiple versions");

            if (!viewModel.ChangeLogs.Any())
                return new MdDocument(GetEmptyBlock());

            // Return changes for only a single change, omit surrounding headers
            return new MdDocument(
                GetVersionContentBlock(viewModel.ChangeLogs.Single())
            );
        }

        /// <inheritdoc />
        protected override MdBlock GetEntryListHeaderBlock(ChangeLogEntryGroupViewModel viewModel)
        {
            // in GitHub releases, the top heading is <h2> because higher,            
            return new MdHeading(2, viewModel.DisplayName);
        }

        /// <inheritdoc />
        protected override MdBlock GetBreakingChangesListHeaderBlock(SingleVersionChangeLogViewModel viewModel)
        {
            // in GitHub releases, the top heading is <h2> because higher,
            return new MdHeading(2, "Breaking Changes");
        }

        /// <inheritdoc />
        protected override MdBlock GetVersionDetailsHeaderBlock(SingleVersionChangeLogViewModel viewModel)
        {
            // in GitHub releases, the top heading is <h2> because higher,
            return new MdHeading(2, "Details");
        }

        /// <inheritdoc />
        protected override MdBlock GetEntryHeaderBlock(ChangeLogEntryViewModel entry)
        {
            // in GitHub releases, the top heading is <h2> because higher,
            // => the header for individual entries is the level of the "details" header + 1 => 3
            return new MdHeading(3, GetEntryTitle(entry)) { Anchor = GetHtmlHeadingId(entry) };
        }

        /// <inheritdoc />
        protected override string GetHtmlHeadingId(ChangeLogEntryViewModel entry) => $"changelog-heading-{entry.Commit}".ToLower();
    }
}
