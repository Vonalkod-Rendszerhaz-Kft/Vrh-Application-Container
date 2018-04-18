using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vrh.CameraService.MessageTypes
{
    public class IOChangeMessageResult
    {
        public IOChangeMessageResult()
        {}

        public IOChangeMessageResult(bool ioState)
        {
            IOState = ioState;
        }

        public bool IOState { get; set; }
    }
}
