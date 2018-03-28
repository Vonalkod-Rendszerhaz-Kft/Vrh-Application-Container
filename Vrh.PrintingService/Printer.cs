using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vrh.PrintingService
{
    public class Printer
    {
        public Printer()
        {

        }

        public Printer(string name, string type, string connectionType, string connectionString)
        {
            Name = name;
            Type = type;
            ConnectionType = connectionType;
            ConnectionString = connectionString;
        }

        public string Name { get; set; }
        public string Type { get; set; }
        public string ConnectionType { get; set; }
        public string ConnectionString { get; set; }

        public ICollection<LabelDefinition> LabelDefinitions{ get; set; } = new HashSet<LabelDefinition>();
    }
}
