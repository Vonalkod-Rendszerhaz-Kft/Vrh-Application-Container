using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vrh.PrintingService
{
    public class LabelDefinition
    {
        public LabelDefinition()
        {
        }

        public LabelDefinition(string name, string printerType, string nameseparator)
        {
            Name = name;
            PrinterType = printerType;
            Nameseparator = nameseparator;
        }

        public string Name { get; set; }
        public string PrinterType { get; set; }
        public string Nameseparator { get; set; }
    }
}
