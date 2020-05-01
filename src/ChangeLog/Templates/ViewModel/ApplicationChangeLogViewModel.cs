using System;
using System.Collections.Generic;
using System.Linq;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Model;

namespace Grynwald.ChangeLog.Templates.ViewModel
{
    /// <summary>
    /// Represents the view of an application's change log
    /// </summary>
    internal class ApplicationChangeLogViewModel
    {
        private readonly ChangeLogConfiguration m_Configuration;
        private readonly ApplicationChangeLog m_Model;

        /// <summary>
        /// Gets all the change log for the individual versions of the application
        /// </summary>
        public IReadOnlyList<SingleVersionChangeLogViewModel> Versions { get; }


        /// <summary>
        /// Initializes a new instance of <see cref="ApplicationChangeLogViewModel" />
        /// </summary>
        public ApplicationChangeLogViewModel(ChangeLogConfiguration configuration, ApplicationChangeLog model)
        {
            m_Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            m_Model = model ?? throw new ArgumentNullException(nameof(model));

            Versions = model.ChangeLogs
                .OrderByDescending(x => x.Version.Version)
                .Select(x => new SingleVersionChangeLogViewModel(configuration, x))
                .ToList();
        }
    }
}
