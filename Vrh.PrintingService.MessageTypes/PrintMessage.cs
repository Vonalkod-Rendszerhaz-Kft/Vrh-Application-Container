using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Vrh.PrintingService.MessageTypes
{
    /// <summary>
    /// 
    /// </summary>
    public class PrintMessage
    {
        public PrintMessage()
        {
        }

        public PrintMessage(string printerName, Message message)
        {
            PrinterName = printerName;
            Message = message;
            SendingState = false;
        }

        public string PrinterName { get; set; }
        public Message Message { get; set; }
        [JsonIgnore]
        public bool SendingState { get; set; }
        [JsonIgnore]
        public Exception Exception { get; set; }
        [JsonIgnore]
        public AutoResetEvent Semaphore { get; set; }
    }
}
