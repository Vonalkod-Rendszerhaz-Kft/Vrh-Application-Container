using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vrh.ApplicationContainer.Core;
using Vrh.Logger;
using System.Messaging;
using System.IO;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;

namespace Vrh.ApplicationContainer.ConsoleHost
{
    class Program
    {
        static void Main(string[] args)
        {
            VRH.Common.CommandLine.SetAppConfigFile(VRH.Common.CommandLine.GetCommandLineArgument(args, "-APPCONFIG"));
            if (!Debugger.IsAttached)
            {
                Console.WriteLine("Attach the debugger now if need and press a key here to continue...");
                Console.ReadLine();
            }
            Core.ApplicationContainer appC = null;
            try
            {
                appC = new Core.ApplicationContainer(args);
            }
            catch (FatalException ex)
            {
                VrhLogger.Log(ex, typeof(Program), LogLevel.Fatal);
            }
            Thread.Sleep(3000);                       
            Console.WriteLine("Application container Started.");
            Console.WriteLine("Press enter to Dispose");
            Console.ReadLine();
            if (appC !=  null)
            {
                appC.Dispose();
            }
            Thread.Sleep(3000);
            Console.WriteLine("Application container disposed.");
            Console.WriteLine("Press enter to Exit");
            Console.ReadLine();
        }
    }
}
