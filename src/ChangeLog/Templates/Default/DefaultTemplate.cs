using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Model;
using Grynwald.MarkdownGenerator;

namespace Grynwald.ChangeLog.Templates.Default
{
    /// <summary>
    /// Implementation of the default template to convert a changelog to Markdown
    /// </summary>
    internal class DefaultTemplate : MarkdownBaseTemplate
    {
        /// <inheritdoc />
        protected override MdSerializationOptions SerializationOptions { get; }

        /// <inheritdoc />
        protected override bool EnableNormalization => m_Configuration.Template.Default.NormalizeReferences;


        public DefaultTemplate(ChangeLogConfiguration configuration) : base(configuration)
        {
            SerializationOptions = MdSerializationOptions.Presets
                .Get(configuration.Template.Default.MarkdownPreset.ToString())
                .With(opts => { opts.HeadingAnchorStyle = MdHeadingAnchorStyle.Auto; });
        }


        /// <inheritdoc />
        protected override string GetHtmlHeadingId(ChangeLogEntry entry) => $"changelog-heading-{entry.Commit}".ToLower();
    }
}
