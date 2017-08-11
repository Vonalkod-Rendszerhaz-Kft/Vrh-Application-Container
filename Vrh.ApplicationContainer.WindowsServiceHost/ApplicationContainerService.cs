using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Vrh.ApplicationContainer.WindowsServiceHost
{
    public partial class ApplicationContainerService : ServiceBase
    {
        public ApplicationContainerService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (_applicationContainer != null)
            {
                _applicationContainer.Dispose();
                _applicationContainer = null;
            }
            _applicationContainer = new ApplicationContainer();
        }

        protected override void OnStop()
        {
            _applicationContainer.Dispose();    
        }

        private ApplicationContainer _applicationContainer = null;
    }
}
