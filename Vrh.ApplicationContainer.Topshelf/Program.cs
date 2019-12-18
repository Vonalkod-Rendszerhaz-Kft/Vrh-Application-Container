using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;
using Vrh.ApplicationContainer.Core;
using VRH.Log4Pro.TopshelfStarter;

namespace Vrh.ApplicationContainer.Topshelf
{
	class Program
	{
		static void Main(string[] args)
		{
			var p = new TopshelfStarter.Params()
			{
				ServiceName = "VRH ApplicationContainer",
				ArgNames = new List<string>() { "INUSEBY", "APPCONFIG" },
			};
			TopshelfStarter.RunDebug<Vrh.ApplicationContainer.Core.ApplicationContainer>(p,args);
		}
	}
}