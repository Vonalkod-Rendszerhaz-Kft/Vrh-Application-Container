using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vrh.Redis.DataPoolHandler;

namespace Vrh.DataToRedisCore
{
    /// <summary>
    /// 
    /// </summary>
    class ExtendedOneData
    {
        public ExtendedOneData()
        {
            OneData = new OneData();
        }

        public OneData OneData { get; set; }
        public string InstanceName { get; set; }
    }
}
