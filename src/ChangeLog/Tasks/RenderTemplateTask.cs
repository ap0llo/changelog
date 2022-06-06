using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Pipeline;
using Grynwald.ChangeLog.Templates;
using Microsoft.Extensions.Logging;

namespace Grynwald.ChangeLog.Tasks
{
    //TODO: Consider moving this class to the "Templates" namespace
    internal sealed class RenderTemplateTask : SynchronousChangeLogTask
    {
        private readonly ILogger<RenderTemplateTask> m_Logger;
        private readonly ChangeLogConfiguration m_Configuration;
        private readonly IReadOnlyList<ITemplate> m_Templates;


        /// <summary>
        /// Initializes a new instance of <see cref="RenderTemplateTask"/>.
        /// </summary>        
        public RenderTemplateTask(ILogger<RenderTemplateTask> logger, ChangeLogConfiguration configuration, IEnumerable<ITemplate> templates)
        {
            if (templates is null)
                throw new ArgumentNullException(nameof(templates));

            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            m_Templates = templates.ToList();
        }


        protected override ChangeLogTaskResult Run(ApplicationChangeLog changeLog)
        {
            var outputPath = m_Configuration.GetFullOutputPath();

            var outputDirectory = Path.GetDirectoryName(outputPath);
            Directory.CreateDirectory(outputDirectory!);

            var template = TryGetTemplate();

            if (template is null)
            {
                return ChangeLogTaskResult.Error;
            }

            m_Logger.LogInformation($"Saving changelog to '{outputPath}'");
            m_Logger.LogDebug($"Using template '{template.Name}'");
            try
            {
                template.SaveChangeLog(changeLog, outputPath);
            }
            catch (TemplateExecutionException ex)
            {
                m_Logger.LogError($"Rendering changelog using template '{template.Name}' failed: {ex.Message}");
                return ChangeLogTaskResult.Error;
            }

            return ChangeLogTaskResult.Success;
        }


        private ITemplate? TryGetTemplate()
        {
            var templateName = m_Configuration.Template.Name;
            var matchingTemplates = m_Templates.Where(x => x.Name == templateName).ToArray();

            switch (matchingTemplates.Length)
            {
                case 0:
                    m_Logger.LogError($"Template '{templateName}' was not found");
                    return null;

                case 1:
                    return matchingTemplates[0];

                case > 1:
                    m_Logger.LogError($"Found multiple templates named '{templateName}'");
                    return null;

                default:
                    // Count cannot be < 0
                    throw new InvalidOperationException();
            }


        }
    }
}
