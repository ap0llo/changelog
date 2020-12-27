using System;
using System.Collections.Generic;
using System.Linq;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Model.Text;
using Grynwald.ChangeLog.Templates.ViewModel;
using Grynwald.MarkdownGenerator;

namespace Grynwald.ChangeLog.Templates
{
    /// <summary>
    /// Base implementation for Markdown templates
    /// </summary>
    /// <seealso cref="Default.DefaultTemplate"/>
    /// <seealso cref="GitLabRelease.GitLabReleaseTemplate"/>
    internal abstract class MarkdownBaseTemplate : ITemplate
    {
        protected readonly ChangeLogConfiguration m_Configuration;

        /// <summary>
        /// Gets the serialization options used for generating Markdown.
        /// </summary>
        protected abstract MdSerializationOptions SerializationOptions { get; }


        public MarkdownBaseTemplate(ChangeLogConfiguration configuration)
        {
            m_Configuration = configuration ?? throw new System.ArgumentNullException(nameof(configuration));
        }


        /// <inheritdoc />
        public virtual void SaveChangeLog(ApplicationChangeLog changeLog, string outputPath)
        {
            var document = GetChangeLogDocument(changeLog);
            document.Save(outputPath, SerializationOptions);
        }

        /// <summary>
        /// Gets the HTML id for the details section of the specified entry
        /// </summary>
        protected abstract string GetHtmlHeadingId(ChangeLogEntry entry);


        /// <summary>
        /// Generates a Markdown document from the specified change log
        /// </summary>
        protected virtual MdDocument GetChangeLogDocument(ApplicationChangeLog model)
        {
            return new MdDocument(
                GetChangeLogHeaderBlock(model),
                GetChangeLogContentBlock(model)
            );
        }

        /// <summary>
        /// Gets the change log's head block
        /// </summary>
        protected virtual MdBlock GetChangeLogHeaderBlock(ApplicationChangeLog model)
        {
            return new MdHeading(1, "Change Log");
        }

        /// <summary>
        /// Gets the change log's content block
        /// </summary>
        protected virtual MdBlock GetChangeLogContentBlock(ApplicationChangeLog model)
        {
            var container = new MdContainerBlock();

            // for each version, add the changes to the document
            // (changeLog.ChangeLogs is ordered by version)
            var firstElement = true;
            foreach (var versionChangeLog in model.ChangeLogs)
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
        protected virtual MdBlock GetVersionBlock(SingleVersionChangeLog model)
        {
            var viewModel = new SingleVersionChangeLogViewModel(m_Configuration, model);

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
            var entryCount = viewModel.AllEntries.Count();

            return entryCount switch
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
            IEnumerable<MdBlock> EnumerateBlocks()
            {
                foreach (var group in viewModel.EntryGroups)
                {
                    yield return GetSummaryListBlock(group);

                }
                yield return GetBreakingChangesListBlock(viewModel);
            }

            return new MdContainerBlock(EnumerateBlocks());
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
            return new MdHeading(3, viewModel.DisplayName);
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
        protected virtual MdBlock GetBreakingChangesListBlock(SingleVersionChangeLogViewModel viewModel)
        {
            if (viewModel.BreakingChanges.Any())
            {
                return new MdContainerBlock(
                    GetBreakingChangesListHeaderBlock(viewModel),
                    GetBreakingChangesListContentBlock(viewModel)
                );
            }

            return MdEmptyBlock.Instance;
        }

        /// <summary>
        /// Gets the header block for a breaking changes list
        /// </summary>
        protected virtual MdBlock GetBreakingChangesListHeaderBlock(SingleVersionChangeLogViewModel viewModel)
        {
            return new MdHeading(3, "Breaking Changes");
        }

        /// <summary>
        /// Gets the content block for the breaking changes list for the specified entries
        /// </summary>
        /// <param name="entries"></param>
        /// <returns></returns>
        protected virtual MdBlock GetBreakingChangesListContentBlock(SingleVersionChangeLogViewModel viewModel)
        {
            var breakingChangesList = new MdBulletList();

            foreach (var entry in viewModel.BreakingChanges)
            {
                // If descriptions for breaking changes were provided,
                // add the descriptions to the list of breaking changes instead of
                // the entry description.
                // A single changelog entry may contain multiple breaking changes

                if (entry.BreakingChangeDescriptions.Any())
                {
                    var link = $"#{GetHtmlHeadingId(entry)}";

                    foreach (var description in entry.BreakingChangeDescriptions)
                    {
                        breakingChangesList.Add(
                            new MdListItem(
                                new MdLinkSpan(description, link)));
                    }
                }
                else
                {
                    // no breaking changes description provided
                    // => add "normal" summary of changelog entry to list of breaking changes
                    breakingChangesList.Add(GetSummaryListItem(entry));
                }
            }

            return breakingChangesList;
        }

        /// <summary>
        /// Gets a detail block for the specified change log entry
        /// </summary>
        protected virtual MdBlock GetEntryDetailBlock(ChangeLogEntry entry)
        {
            return new MdContainerBlock(
                GetEntryDetailHeaderBlock(entry),
                GetEntryDetailContentBlock(entry)
            );
        }

        /// <summary>
        /// Gets the header block of the details block for the specified entry
        /// </summary>
        protected virtual MdBlock GetEntryDetailHeaderBlock(ChangeLogEntry entry)
        {
            return new MdHeading(4, GetSummaryText(entry)) { Anchor = GetHtmlHeadingId(entry) };
        }

        /// <summary>
        /// Gets the content block of the details block for the specified entry
        /// </summary>
        protected virtual MdBlock GetEntryDetailContentBlock(ChangeLogEntry entry)
        {
            var block = new MdContainerBlock();

            // highlight breaking changes
            if (entry.ContainsBreakingChanges)
            {
                // If descriptions for breaking changes were provided,
                // add all descriptions, prefixed with "Breaking Change" to the output
                if (entry.BreakingChangeDescriptions.Any())
                {
                    // Prefix all descriptions with a bold "Breaking Change"
                    var descriptions = entry.BreakingChangeDescriptions
                        .Select(x => new MdCompositeSpan(
                                        new MdStrongEmphasisSpan("Breaking Change:"),
                                        new MdTextSpan(" "),
                                        new MdTextSpan(x)))
                        .ToList();

                    // if there is only a single description, add it to the output as paragraph
                    if (descriptions.Count == 1)
                    {
                        block.Add(new MdParagraph(descriptions.Single()));
                    }
                    // if there are multiple descriptions, add them to the output as list
                    else
                    {
                        block.Add(new MdBulletList(descriptions.Select(x => new MdListItem(x))));
                    }
                }
                // If no description was provided but the entry itself was marked as breaking change
                // add a plain "Breaking Change" hint to the output
                else
                {
                    block.Add(new MdParagraph(new MdStrongEmphasisSpan("Breaking Change")));
                }
            }

            // Add body of commit message
            foreach (var paragraph in entry.Body)
            {
                block.Add(new MdParagraph(paragraph));
            }

            // add additional information
            var footerList = new MdBulletList();
            block.Add(footerList);

            foreach (var footer in entry.Footers)
            {
                // Rendering footers:
                //  - Footers consists of a name and a value.
                //  - By default, footers are rendered as a bullet list where each item uses the format <NAME>: <VALUE>
                //  - Footer values are implementations of ITextElement but might be custom implementations.
                //    Three special cases are handled here
                //      - If the footer value's style is "Code" the footer value is rendered as Markdown code span
                //      - If the footer value implements IWebLinkTextElement (no matter what the actual type is),
                //        the footer is rendered as a Markdown link
                //      - If the footer is an instance of ChangeLogEntryReferenceTextElement,
                //        the footer is rendered as an intra-page link to the referenced change log entry.
                //        In that case, the referenced entry's summary is used instead of the footer's value in the output

                MdSpan text = footer.Value.Style == TextStyle.Code
                    ? new MdCodeSpan(footer.Value.Text)
                    : footer.Value.Text;

                if (footer.Value is IWebLinkTextElement webLink)
                {
                    text = new MdLinkSpan(text, webLink.Uri);
                }
                else if (footer.Value is ChangeLogEntryReferenceTextElement entryReference)
                {
                    var id = GetHtmlHeadingId(entryReference.Entry);
                    text = new MdLinkSpan(GetSummaryText(entryReference.Entry), $"#{id}");
                }

                footerList.Add(
                    new MdListItem($"{footer.GetFooterDisplayName(m_Configuration)}: ", text)
                );
            }

            //TODO: Move this to the model class (an "implicit footer")
            MdSpan commitText = new MdCodeSpan(entry.Commit.ToString(abbreviate: true));
            if (entry.CommitWebUri != null)
            {
                commitText = new MdLinkSpan(commitText, entry.CommitWebUri);
            }

            footerList.Add(
                new MdListItem("Commit: ", commitText)
            );

            return block;
        }

        /// <summary>
        /// Gets a list item for the specified changelog entry
        /// </summary>
        private MdListItem GetSummaryListItem(ChangeLogEntry entry)
        {
            var text = GetSummaryText(entry);
            var id = GetHtmlHeadingId(entry);

            // make the list item a link to the details for this changelog entry
            return new MdListItem(
                new MdLinkSpan(text, $"#{id}")
            );
        }

        protected MdSpan GetSummaryText(ChangeLogEntry entry)
        {
            var scope = entry.GetScopeDisplayName(m_Configuration);

            return scope switch
            {
                string s when !String.IsNullOrEmpty(s) =>
                    new MdCompositeSpan(
                        new MdStrongEmphasisSpan($"{scope}:"),
                        new MdTextSpan($" {entry.Summary}")),

                _ => new MdTextSpan(entry.Summary),
            };
        }


    }
}
