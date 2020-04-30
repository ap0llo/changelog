using System;
using System.Collections.Generic;
using System.Linq;
using Grynwald.ChangeLog.Model;

namespace Grynwald.ChangeLog.Templates.ViewModel
{
    //TODO: Use DI container?
    internal class ApplicationChangeLogViewModel
    {
        public IReadOnlyList<SingleVersionChangeLogViewModel> Versions { get; }


        /// <summary>
        /// Initializes a new instance of <see cref="ApplicationChangeLogViewModel" />
        /// </summary>
        public ApplicationChangeLogViewModel(ApplicationChangeLog model)
        {
            if (model is null)
                throw new ArgumentNullException(nameof(model));

            Versions = model.ChangeLogs
                .OrderByDescending(x => x.Version.Version)
                .Select(x => new SingleVersionChangeLogViewModel(x))
                .ToList();
        }
    }
}
