using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace TEST
{
    class Program
    {
        static void Main(string[] args)
        {
            var cm = ConnectionMultiplexer.Connect("localhost:6379,defaultDatabase=7");
            var redisDb = cm?.GetDatabase();
            if (redisDb != null)
            {
                while (true)
                {

                    System.Threading.Thread.Sleep(2000);
                    Console.Clear();
                    //Console.ReadLine();
                    List<string> servicenameList = new List<string>()
                    {
                        "VRH Starter for ALM"
                        , "VRH DataController WPWF for ALM"
                        , "VRH ASEDC_ASEMON1 for ALM"
                        , "VRH ASEDC_ASEMON2 for ALM"
                        , "VRH ASEMW for ALM"
                        , "VRH iLogger for ALM"
                        , "VRH AppContainer APP for ALM"
                        , "VRH AppContainer IVC for ALM"
                        , "VRH AppContainer ISCH for ALM"
                        , "VRH Terminator for ALM"
                        , "VRH Redis for ALM"
                    };
                    foreach (string servicename in servicenameList)
                    {
                        string semaforname = $"Service.Starter.Semafor.{servicename}";
                        var semaforvalue = redisDb.StringGet(semaforname);
                        if (semaforvalue.IsNullOrEmpty) semaforvalue = "EMPTY";
                        Console.WriteLine($"{semaforname.PadRight(60)}: {semaforvalue}");
                    }
                }
            }
        }
    }
}
