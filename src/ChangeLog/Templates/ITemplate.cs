﻿using Grynwald.ChangeLog.Model;

namespace Grynwald.ChangeLog.Templates
{
    /// <summary>
    /// Interface for templates that generate a output file from the changelog model.
    /// </summary>
    public interface ITemplate
    {
        /// <summary>
        /// Gets the template's name
        /// </summary>
        TemplateName Name { get; }

        /// <summary>
        /// Saves the changelog to the specified path
        /// </summary>
        /// <param name="changeLog">The changelog to save to disk.</param>
        /// <param name="outputPath">The path to write the changelog to.</param>
        /// <exception cref="TemplateExecutionException">Thrown if an error occurred while rendering the change log using the current template.</exception>
        void SaveChangeLog(ApplicationChangeLog changeLog, string outputPath);
    }
}
