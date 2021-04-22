using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Model.Text;
using Grynwald.ChangeLog.Templates;
using Grynwald.ChangeLog.Templates.Html;
using Xunit;

namespace Grynwald.ChangeLog.Test.Templates.Html
{
    public class HtmlTemplateTest : TemplateTest
    {
        protected override ITemplate GetTemplateInstance(ChangeLogConfiguration configuration) => new HtmlTemplate(configuration);


        //TODO: Add support for normalization, option to disable normalization
    }
}
