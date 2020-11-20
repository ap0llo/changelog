using System;
using System.IO;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Model;
using Grynwald.ChangeLog.Templates;
using Microsoft.Extensions.Logging;

namespace Grynwald.ChangeLog.Tasks
{
    //TODO: Consider moving this class to the "Templates" namespace
    internal sealed class RenderTemplateTask : SynchronousChangeLogTask
    {
        private readonly ILogger<RenderTemplateTask> m_Logger;
        private readonly ChangeLogConfiguration m_Configuration;
        private readonly ITemplate m_Template;


        /// <summary>
        /// Initializes a new instance of <see cref="RenderTemplateTask"/>.
        /// </summary>
        /// <param name="outputPath">The file path to save the changelog to.</param>
        public RenderTemplateTask(ILogger<RenderTemplateTask> logger, ChangeLogConfiguration configuration, ITemplate template)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            m_Template = template ?? throw new ArgumentNullException(nameof(template));
        }


        protected override ChangeLogTaskResult Run(ApplicationChangeLog changeLog)
        {
            var outputPath = m_Configuration.GetFullOutputPath();

            var outputDirectory = Path.GetDirectoryName(outputPath);
            Directory.CreateDirectory(outputDirectory!);

            m_Logger.LogInformation($"Saving changelog to '{outputPath}'");
            try
            {
                m_Template.SaveChangeLog(changeLog, outputPath);
            }
            catch (TemplateExecutionException ex)
            {
                m_Logger.LogError($"Rendering changelog using template '{m_Template.GetType().Name}' failed: {ex.Message}");
                return ChangeLogTaskResult.Error;
            }

            return ChangeLogTaskResult.Success;
        }
    }
}
