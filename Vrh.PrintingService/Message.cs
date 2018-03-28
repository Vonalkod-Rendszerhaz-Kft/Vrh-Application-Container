using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vrh.PrintingService
{
    public class Message
    {
        public Message()
        {
        }

        public Message(string labelFormatName, string stream, Dictionary<string, string> dataList, Modes mode)
        {
            LabelDefinitionName = labelFormatName;
            Stream = stream;
            DataList = dataList;
            Mode = mode;
        }

        public string LabelDefinitionName { get; set; }
        public string Stream { get; set; }
        public Dictionary<string, string> DataList = new Dictionary<string, string>();
        public Modes Mode { get; set; }
    }
}
