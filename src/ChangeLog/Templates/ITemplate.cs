using Grynwald.ChangeLog.Model;

namespace Grynwald.ChangeLog.Templates
{
    /// <summary>
    /// Interface for templates that generate a output file from the changelog model.
    /// </summary>
    internal interface ITemplate
    {
        /// <summary>
        /// Saves the changelog to the specified path
        /// </summary>
        /// <param name="changeLog">The changelog to save to disk.</param>
        /// <param name="outputPath">The path to write the changelog to.</param>
        void SaveChangeLog(ApplicationChangeLog changeLog, string outputPath);
    }
}
