using System;
using System.Collections.Generic;
using System.Linq;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Model;
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
            return new MdContainerBlock(
                GetVersionHeaderBlock(model),
                GetVersionContentBlock(model)
            );
        }

        /// <summary>
        /// Gets the header block for the specified version change log
        /// </summary>
        protected virtual MdBlock GetVersionHeaderBlock(SingleVersionChangeLog model)
        {
            var version = model.Version.Version;
            return new MdHeading(2, version.ToNormalizedString());
        }

        /// <summary>
        /// Gets the content block for the specified version change log
        /// </summary>
        protected virtual MdBlock GetVersionContentBlock(SingleVersionChangeLog model)
        {
            var features = model.FeatureEntries.ToArray();
            var bugFixes = model.BugFixEntries.ToArray();
            var allBreakingChanges = model.BreakingChanges.ToArray();
            var additionalBreakingChanges = allBreakingChanges.Except(features).Except(bugFixes).ToArray();  // breaking changes not in 'features' or 'bugFixes'

            var entryCount = features.Length + bugFixes.Length + additionalBreakingChanges.Length;

            var allEntries = features.Concat(bugFixes).Concat(additionalBreakingChanges);

            return entryCount switch
            {
                0 => GetEmptyBlock(),
                1 => GetEntryDetailBlock(allEntries.Single()),
                _ => new MdContainerBlock(
                    GetSummarySectionBlock(model.Version, features, bugFixes, allBreakingChanges),
                    GetDetailSectionBlock(model.Version, allEntries)
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
        protected virtual MdBlock GetSummarySectionBlock(VersionInfo versionInfo, IEnumerable<ChangeLogEntry> features, IEnumerable<ChangeLogEntry> bugfixes, IEnumerable<ChangeLogEntry> breakingChanges)
        {
            return new MdContainerBlock(
                GetSummaryListBlock("New Features", features),
                GetSummaryListBlock("Bug Fixes", bugfixes),
                GetBreakingChangesListBlock(versionInfo, breakingChanges)
            );
        }

        /// <summary>
        /// Gets the details section for a version changelog
        /// </summary>
        protected virtual MdBlock GetDetailSectionBlock(VersionInfo versionInfo, IEnumerable<ChangeLogEntry> entries)
        {
            return new MdContainerBlock(
                GetDetailSectionHeaderBlock(versionInfo.Version),
                GetDetailSectionContentBlock(entries)
            );
        }

        /// <summary>
        /// Gets the header block of the version change log's details section
        /// </summary>
        protected virtual MdBlock GetDetailSectionHeaderBlock(NuGetVersion version)
        {
            return new MdHeading(3, "Details");
        }

        /// <summary>
        /// Gets the content block of the version change log's details section
        /// </summary>
        protected virtual MdBlock GetDetailSectionContentBlock(IEnumerable<ChangeLogEntry> entries)
        {
            return new MdContainerBlock(
                    entries.Select(GetEntryDetailBlock)
                );
        }

        /// <summary>
        /// Gets a summary list block for the specified changelog entries
        /// </summary>
        protected virtual MdBlock GetSummaryListBlock(string listTitle, IEnumerable<ChangeLogEntry> entries)
        {
            if (entries.Any())
            {
                return new MdContainerBlock(
                    GetSummaryListHeaderBlock(listTitle),
                    GetSummaryListContentBlock(entries)
                );
            }

            return MdEmptyBlock.Instance;
        }

        /// <summary>
        /// Gets the header for a summary list block
        /// </summary>
        protected virtual MdBlock GetSummaryListHeaderBlock(string listTitle)
        {
            return new MdHeading(3, listTitle);
        }

        /// <summary>
        /// Gets the content for a summary list block
        /// </summary>
        protected virtual MdBlock GetSummaryListContentBlock(IEnumerable<ChangeLogEntry> entries)
        {
            return new MdBulletList(entries.Select(GetSummaryListItem));
        }

        /// <summary>
        /// Gets a breaking changes list for the specified changes
        /// </summary>
        protected virtual MdBlock GetBreakingChangesListBlock(VersionInfo versionInfo, IEnumerable<ChangeLogEntry> entries)
        {
            if (entries.Any())
            {
                return new MdContainerBlock(
                    GetBreakingChangesListHeaderBlock(versionInfo),
                    GetBreakingChangesListContentBlock(versionInfo, entries)
                );
            }

            return MdEmptyBlock.Instance;
        }

        /// <summary>
        /// Gets the header block for a breaking changes list
        /// </summary>
        protected virtual MdBlock GetBreakingChangesListHeaderBlock(VersionInfo versionInfo)
        {
            return new MdHeading(3, "Breaking Changes");
        }

        /// <summary>
        /// Gets the content block for the breaking changes list for the specified entries
        /// </summary>
        /// <param name="entries"></param>
        /// <returns></returns>
        protected virtual MdBlock GetBreakingChangesListContentBlock(VersionInfo versionInfo, IEnumerable<ChangeLogEntry> entries)
        {
            var breakingChangesList = new MdBulletList();

            foreach (var entry in entries)
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
                MdSpan text = footer.Value;
                if (footer.WebUri != null)
                {
                    text = new MdLinkSpan(text, footer.WebUri);
                }
                footerList.Add(
                    new MdListItem($"{footer.GetFooterDisplayName(m_Configuration)}: ", text)
                );

            }

            MdSpan commitText = new MdCodeSpan(entry.Commit.Id);
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
