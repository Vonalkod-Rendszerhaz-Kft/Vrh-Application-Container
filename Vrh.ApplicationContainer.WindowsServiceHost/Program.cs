using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Vrh.ApplicationContainer.WindowsServiceHost
{
    static class Program
    {
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args)
        {
			Thread.Sleep(10000);

			VRH.Common.CommandLine.SetAppConfigFile(VRH.Common.CommandLine.GetCommandLineArgument(args, "-APPCONFIG"));
			ServiceBase[] ServicesToRun;
			ServicesToRun = new ServiceBase[]
			{
			new ApplicationContainerService(args)
			};
			ServiceBase.Run(ServicesToRun);
        }
    }
}
