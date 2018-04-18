using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vrh.CameraService.MessageTypes
{
    public class Message
    {
        public Message()
        {
        }

        public Message(string labelFormatName, string stream)
        {
            LabelDefinitionName = labelFormatName;
            Stream = stream;
        }

        public string LabelDefinitionName { get; set; }
        public string Stream { get; set; }
    }
}
