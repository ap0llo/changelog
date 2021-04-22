using System;
using System.Collections.Generic;
using System.Linq;
using Grynwald.ChangeLog.Configuration;
using Grynwald.ChangeLog.Model;

namespace Grynwald.ChangeLog.Templates.ViewModel
{
    internal class ApplicationChangeLogViewModel
    {
        private readonly ChangeLogConfiguration m_Configuration;
        private readonly ApplicationChangeLog m_Model;


        public IEnumerable<SingleVersionChangeLogViewModel> ChangeLogs => m_Model.ChangeLogs.Select(x => new SingleVersionChangeLogViewModel(m_Configuration, x));


        public ApplicationChangeLogViewModel(ChangeLogConfiguration configuration, ApplicationChangeLog model)
        {
            m_Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            m_Model = model ?? throw new ArgumentNullException(nameof(model));
        }
    }
}
