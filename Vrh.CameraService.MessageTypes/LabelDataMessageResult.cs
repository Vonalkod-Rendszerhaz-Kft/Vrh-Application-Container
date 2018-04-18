using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vrh.CameraService.MessageTypes
{
    public class LabelDataMessageResult
    {
        public LabelDataMessageResult()
        {}

        public LabelDataMessageResult(string labelData)
        {
            LabelData = labelData;
        }

        public string LabelData { get; set; }
    }
}
