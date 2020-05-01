using System;
using System.Collections.Generic;
using System.Linq;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Templates.ViewModel;
using Grynwald.MarkdownGenerator;
using NuGet.Versioning;

namespace Grynwald.ChangeLog.Templates
{
    /// <summary>
    /// Base implementation for Markdown templates
    /// </summary>
    /// <seealso cref="Default.DefaultTemplate"/>
    /// <seealso cref="GitLabRelease.GitLabReleaseTemplate"/>
    internal abstract class MarkdownBaseTemplate : ITemplate
    {
        private readonly ChangeLogConfiguration m_Configuration;

        /// <summary>
        /// Gets the serialization options used for generating Markdown.
        /// </summary>
        protected abstract MdSerializationOptions SerializationOptions { get; }


        public MarkdownBaseTemplate(ChangeLogConfiguration configuration)
        {
            m_Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }


        /// <inheritdoc />
        public virtual void SaveChangeLog(ApplicationChangeLog changeLog, string outputPath)
        {
            var viewModel = new ApplicationChangeLogViewModel(m_Configuration, changeLog);

            var document = GetChangeLogDocument(viewModel);
            document.Save(outputPath, SerializationOptions);
        }

        /// <summary>
        /// Gets the HTML id for the details section of the specified entry
        /// </summary>
        protected abstract string GetHtmlHeadingId(ChangeLogEntryViewModel viewModel);


        /// <summary>
        /// Generates a Markdown document from the specified change log
        /// </summary>
        protected virtual MdDocument GetChangeLogDocument(ApplicationChangeLogViewModel viewModel)
        {
            return new MdDocument(
                GetChangeLogHeaderBlock(viewModel),
                GetChangeLogContentBlock(viewModel)
            );
        }

        /// <summary>
        /// Gets the change log's head block
        /// </summary>
        protected virtual MdBlock GetChangeLogHeaderBlock(ApplicationChangeLogViewModel viewModel)
        {
            return new MdHeading(1, "Change Log");
        }

        /// <summary>
        /// Gets the change log's content block
        /// </summary>
        protected virtual MdBlock GetChangeLogContentBlock(ApplicationChangeLogViewModel viewModel)
        {
            var container = new MdContainerBlock();

            // for each version, add the changes to the document
            // (changeLog.ChangeLogs is ordered by version)
            var firstElement = true;
            foreach (var versionChangeLog in viewModel.Versions)
            {
                if (!firstElement)
                    container.Add(new MdThematicBreak());

                container.Add(GetVersionBlock(versionChangeLog));
                firstElement = false;
            }

            return container;
        }

        /// <summary>
        /// Gets the Markdown block for the specified version change log
        /// </summary>
        protected virtual MdBlock GetVersionBlock(SingleVersionChangeLogViewModel viewModel)
        {
            return new MdContainerBlock(
                GetVersionHeaderBlock(viewModel),
                GetVersionContentBlock(viewModel)
            );
        }

        /// <summary>
        /// Gets the header block for the specified version change log
        /// </summary>
        protected virtual MdBlock GetVersionHeaderBlock(SingleVersionChangeLogViewModel viewModel)
        {
            return new MdHeading(2, viewModel.VersionDisplayName);
        }

        /// <summary>
        /// Gets the content block for the specified version change log
        /// </summary>
        protected virtual MdBlock GetVersionContentBlock(SingleVersionChangeLogViewModel viewModel)
        {
            return viewModel.AllEntries.Count switch
            {
                0 => GetEmptyBlock(),
                1 => GetEntryDetailBlock(viewModel.AllEntries.Single()),
                _ => new MdContainerBlock(
                    GetSummarySectionBlock(viewModel),
                    GetDetailSectionBlock(viewModel)
                )
            };
        }

        /// <summary>
        /// Gets the block to use in place of a change log when a version change log has no entries
        /// </summary>
        protected virtual MdBlock GetEmptyBlock() => new MdParagraph(new MdEmphasisSpan("No changes found."));

        /// <summary>
        /// Gets the summary section for a version change log
        /// </summary>
        protected virtual MdBlock GetSummarySectionBlock(SingleVersionChangeLogViewModel viewModel)
        {
            var container = new MdContainerBlock();

            foreach(var group in viewModel.EntryGroups)
            {
                container.Add(GetSummaryListBlock(group));
            }
            container.Add(GetBreakingChangesListBlock(viewModel.BreakingChanges));
            
            return container;
        }

        /// <summary>
        /// Gets the details section for a version changelog
        /// </summary>
        protected virtual MdBlock GetDetailSectionBlock(SingleVersionChangeLogViewModel viewModel)
        {
            return new MdContainerBlock(
                GetDetailSectionHeaderBlock(viewModel),
                GetDetailSectionContentBlock(viewModel)
            );
        }

        /// <summary>
        /// Gets the header block of the version change log's details section
        /// </summary>
        protected virtual MdBlock GetDetailSectionHeaderBlock(SingleVersionChangeLogViewModel viewModel)
        {
            return new MdHeading(3, "Details");
        }

        /// <summary>
        /// Gets the content block of the version change log's details section
        /// </summary>
        protected virtual MdBlock GetDetailSectionContentBlock(SingleVersionChangeLogViewModel viewModel)
        {
            return new MdContainerBlock(
                    viewModel.AllEntries.Select(GetEntryDetailBlock)
                );
        }

        /// <summary>
        /// Gets a summary list block for the specified changelog entries
        /// </summary>
        protected virtual MdBlock GetSummaryListBlock(ChangeLogEntryGroupViewModel viewModel)
        {
                return new MdContainerBlock(
                    GetSummaryListHeaderBlock(viewModel),
                    GetSummaryListContentBlock(viewModel)
                );

        }

        /// <summary>
        /// Gets the header for a summary list block
        /// </summary>
        protected virtual MdBlock GetSummaryListHeaderBlock(ChangeLogEntryGroupViewModel viewModel)
        {
            return new MdHeading(3, viewModel.Title);
        }

        /// <summary>
        /// Gets the content for a summary list block
        /// </summary>
        protected virtual MdBlock GetSummaryListContentBlock(ChangeLogEntryGroupViewModel viewModel)
        {
            return new MdBulletList(viewModel.Entries.Select(GetSummaryListItem));
        }

        /// <summary>
        /// Gets a breaking changes list for the specified changes
        /// </summary>
        protected virtual MdBlock GetBreakingChangesListBlock(IEnumerable<BreakingChangeViewModel> viewModels)
        {
            if (viewModels.Any())
            {
                return new MdContainerBlock(
                    GetBreakingChangesListHeaderBlock(),
                    GetBreakingChangesListContentBlock(viewModels)
                );
            }

            return MdEmptyBlock.Instance;
        }

        /// <summary>
        /// Gets the header block for a breaking changes list
        /// </summary>
        protected virtual MdBlock GetBreakingChangesListHeaderBlock()
        {
            return new MdHeading(3, "Breaking Changes");
        }

        /// <summary>
        /// Gets the content block for the breaking changes list for the specified entries
        /// </summary>
        /// <param name="entries"></param>
        /// <returns></returns>
        protected virtual MdBlock GetBreakingChangesListContentBlock(IEnumerable<BreakingChangeViewModel> viewModels)
        {
            return new MdBulletList( viewModels.Select(viewModel =>
                new MdListItem(
                    new MdLinkSpan(viewModel.Description, $"#{GetHtmlHeadingId(viewModel.Entry)}"))
            ));
        }

        /// <summary>
        /// Gets a detail block for the specified change log entry
        /// </summary>
        protected virtual MdBlock GetEntryDetailBlock(ChangeLogEntryViewModel viewModel)
        {
            return new MdContainerBlock(
                GetEntryDetailHeaderBlock(viewModel),
                GetEntryDetailContentBlock(viewModel)
            );
        }

        /// <summary>
        /// Gets the header block of the details block for the specified entry
        /// </summary>
        protected virtual MdBlock GetEntryDetailHeaderBlock(ChangeLogEntryViewModel viewModel)
        {
            return new MdHeading(4, viewModel.Title) { Anchor = GetHtmlHeadingId(viewModel) };
        }

        /// <summary>
        /// Gets the content block of the details block for the specified entry
        /// </summary>
        protected virtual MdBlock GetEntryDetailContentBlock(ChangeLogEntryViewModel viewModel)
        {
            var block = new MdContainerBlock();

            var breakingChanges = viewModel.BreakingChanges
                 .Select(x =>
                 {
                     if (x.IsBreakingChangeFromHeader)
                         return new MdStrongEmphasisSpan("Breaking Change");

                     return (MdSpan) new MdCompositeSpan(
                         new MdStrongEmphasisSpan("Breaking Change:"),
                         new MdTextSpan(" "),
                         new MdTextSpan(x.Description)
                    );
                 })
                 .ToArray();

            block.Add(breakingChanges.Length switch
            {
                0 => MdEmptyBlock.Instance,
                1 => new MdParagraph(breakingChanges[0]),
                _ => new MdBulletList(breakingChanges.Select(x => new MdListItem(x)))
            });

             
            // Add body of commit message
            foreach (var paragraph in viewModel.Body)
            {
                block.Add(new MdParagraph(paragraph));
            }

            // Add footers
            if(viewModel.Footers.Count > 0)
            {
                var footerList = new MdBulletList();
                block.Add(footerList);

                foreach (var footer in viewModel.Footers)
                {
                    MdSpan text = footer.Value;
                    if (footer.WebUri != null)
                    {
                        text = new MdLinkSpan(text, footer.WebUri);
                    }
                    footerList.Add(
                        new MdListItem($"{footer.DisplayName}: ", text)
                    );
                }
            }
     
            return block;
        }

        /// <summary>
        /// Gets a list item for the specified changelog entry
        /// </summary>
        private MdListItem GetSummaryListItem(ChangeLogEntryViewModel viewModel)
        {            
            var id = GetHtmlHeadingId(viewModel);

            // make the list item a link to the details for this changelog entry
            return new MdListItem(
                new MdLinkSpan(viewModel.Title, $"#{id}")
            );
        }

        

    }
}
