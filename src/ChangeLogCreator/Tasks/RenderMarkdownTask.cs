using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ChangeLogCreator.Configuration;
using ChangeLogCreator.Model;
using Grynwald.MarkdownGenerator;

namespace ChangeLogCreator.Tasks
{
    internal sealed class RenderMarkdownTask : IChangeLogTask
    {
        private const string s_HeadingIdPrefix = "changelog-heading";
        private readonly ChangeLogConfiguration m_Configuration;


        /// <summary>
        /// Gets the serialization options used for generating Markdown.
        /// </summary>
        internal MdSerializationOptions SerializationOptions { get; }


        /// <summary>
        /// Initializes a new instance of <see cref="RenderMarkdownTask"/>.
        /// </summary>
        /// <param name="outputPath">The file path to save the changelog to.</param>
        public RenderMarkdownTask(ChangeLogConfiguration configuration)
        {
            m_Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            SerializationOptions = MdSerializationOptions.Presets
                .Get(configuration.Markdown.Preset.ToString())
                .With(opts => { opts.HeadingAnchorStyle = MdHeadingAnchorStyle.Tag; });
        }


        /// <inheritdoc />
        public Task RunAsync(ChangeLog changeLog)
        {
            var document = GetChangeLogDocument(changeLog);

            var outputPath = m_Configuration.GetFullOutputPath();

            var outputDirectory = Path.GetDirectoryName(outputPath);
            Directory.CreateDirectory(outputDirectory);

            document.Save(outputPath, SerializationOptions);

            return Task.CompletedTask;
        }


        /// <summary>
        /// Gets the Markdown representation of the specified changelog without saving it to a file
        /// </summary>
        internal MdDocument GetChangeLogDocument(ChangeLog changeLog)
        {
            var document = new MdDocument();
            document.Root.AddHeading(1, "Change Log", $"{s_HeadingIdPrefix}-root");

            // for each version, add the changes to the document
            // (changeLog.ChangeLogs is ordered by version)
            var firstElement = true;
            foreach (var versionChangeLog in changeLog.ChangeLogs)
            {
                if (!firstElement)
                    document.Root.Add(new MdThematicBreak());

                document.Root.Add(GetVersionSection(versionChangeLog));
                firstElement = false;
            }

            return document;
        }


        /// <summary>
        /// Gets the Markdown representation of the changes of a single version.
        /// </summary>
        private MdBlock GetVersionSection(SingleVersionChangeLog versionChangeLog)
        {
            var container = new MdContainerBlock();
            container.AddHeading(2, versionChangeLog.Version.Version.ToNormalizedString(), GetHtmlHeadingId(versionChangeLog));


            var features = versionChangeLog.FeatureEntries.ToArray();
            var bugFixes = versionChangeLog.BugFixEntries.ToArray();
            var allBreakingChanges = versionChangeLog.BreakingChanges.ToArray();
            var additionalBreakingChanges = allBreakingChanges.Except(features).Except(bugFixes).ToArray();  // breaking changes not in 'features' or 'bugFixes'

            var entryCount = features.Length + bugFixes.Length + additionalBreakingChanges.Length;

            if (entryCount == 0)
            {
                container.Add(new MdParagraph(new MdEmphasisSpan("No changes found.")));
            }
            else if (entryCount == 1)
            {
                // use simpler layout for versions that only contain a single changelog entry:
                // - omit the list of changes at the beginning
                // - omit the "Details" heading and directly insert the details section

                foreach (var feature in features)
                {
                    container.Add(GetDetailsBlock(feature));
                }
                foreach (var bugFix in bugFixes)
                {
                    container.Add(GetDetailsBlock(bugFix));
                }
                foreach (var breakingChange in additionalBreakingChanges)
                {
                    container.Add(GetDetailsBlock(breakingChange));
                }
            }
            else
            {
                if (features.Length > 0)
                {
                    container.AddHeading(3, "New Features", GetHtmlHeadingId(versionChangeLog, "features"));
                    container.Add(new MdBulletList(features.Select(GetSummaryListItem)));
                }

                if (bugFixes.Length > 0)
                {
                    container.AddHeading(3, "Bug Fixes", GetHtmlHeadingId(versionChangeLog, "bugfixes"));
                    container.Add(new MdBulletList(bugFixes.Select(GetSummaryListItem)));
                }

                if (allBreakingChanges.Length > 0)
                {
                    container.AddHeading(3, "Breaking Changes", GetHtmlHeadingId(versionChangeLog, "breaking"));

                    var breakingChangesList = container.AddBulletList();

                    foreach (var entry in allBreakingChanges)
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

                }

                //TODO: Option to omit details section

                container.AddHeading(3, "Details", GetHtmlHeadingId(versionChangeLog, "details"));
                foreach (var feature in features)
                {
                    container.Add(GetDetailsBlock(feature));
                }
                foreach (var bugFix in bugFixes)
                {
                    container.Add(GetDetailsBlock(bugFix));
                }
                foreach (var breakingChange in additionalBreakingChanges)
                {
                    container.Add(GetDetailsBlock(breakingChange));
                }
            }

            return container;
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


        private MdSpan GetSummaryText(ChangeLogEntry entry)
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

        /// <summary>
        /// Gets the Markdown block of the specified entry for the details section
        /// </summary>
        private MdBlock GetDetailsBlock(ChangeLogEntry entry)
        {
            var block = new MdContainerBlock();
            block.AddHeading(4, GetSummaryText(entry), GetHtmlHeadingId(entry));

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
            var footerList = block.AddBulletList();

            foreach(var footer in entry.Footers)  
            {
                MdSpan text = footer.Value;
                if(footer.WebUri != null)
                {
                    text = new MdLinkSpan(text, footer.WebUri);
                }
                footerList.Add(
                    new MdListItem($"{footer.GetFooterDisplayName(m_Configuration)}: ", text)
                );

            }
            //TODO: Move this to the model class (an "implicit footer)
            MdSpan commitText = new MdCodeSpan(entry.Commit.Id);
            if(entry.CommitWebUri != null)
            {
                commitText = new MdLinkSpan(commitText, entry.CommitWebUri);
            }

            footerList.Add(
                new MdListItem("Commit: ", commitText)
            );

            return block;
        }

        private string GetHtmlHeadingId(ChangeLogEntry entry) => $"{s_HeadingIdPrefix}-{entry.Commit}";

        private string GetHtmlHeadingId(SingleVersionChangeLog changelog, string? suffix = null)
        {
            var versionSlug = HtmlUtilities.ToUrlSlug(changelog.Version.Version.ToNormalizedString());

            var id = $"{s_HeadingIdPrefix}-{versionSlug}";
            if (suffix != null)
            {
                id += "-" + suffix;
            }

            return id;
        }
    }
}
