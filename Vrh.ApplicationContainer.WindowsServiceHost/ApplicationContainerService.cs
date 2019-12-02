using System.ServiceProcess;

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
            _applicationContainer = new Core.ApplicationContainer(args);
        }

        protected override void OnStop()
        {
            _applicationContainer.Dispose();    
        }

        private Core.ApplicationContainer _applicationContainer = null;
        private string[] commandlinearguments;
    }
}
