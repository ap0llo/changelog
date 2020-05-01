using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Templates.ViewModel;
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


        public DefaultTemplate(ChangeLogConfiguration configuration) : base(configuration)
        {
            SerializationOptions = MdSerializationOptions.Presets
                .Get(configuration.Template.Default.MarkdownPreset.ToString())
                .With(opts => { opts.HeadingAnchorStyle = MdHeadingAnchorStyle.Auto; });
        }


        /// <inheritdoc />
        protected override string GetHtmlHeadingId(ChangeLogEntryViewModel viewModel) => $"changelog-heading-{viewModel.Commit}";
    }
}
