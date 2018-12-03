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
        public ApplicationContainerService(string[] args)
        {
            InitializeComponent();
            commandlinearguments = args; /// elmentjük a szerviz regisztrációban megadott parancssori paramétereket
        }

        protected override void OnStart(string[] args)
        {
            args = (args == null || args.Length == 0 ? commandlinearguments : args);
            if (_applicationContainer != null)
            {
                _applicationContainer.Dispose();
                _applicationContainer = null;
            }
            _applicationContainer = new ApplicationContainer(args);
        }

        protected override void OnStop()
        {
            _applicationContainer.Dispose();    
        }

        private ApplicationContainer _applicationContainer = null;
        private string[] commandlinearguments;
    }
}
