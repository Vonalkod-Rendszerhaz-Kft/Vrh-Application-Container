﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vrh.PrintingService
{
    /// <summary>
    /// 
    /// </summary>
    public class PrintMessage
    {
        public PrintMessage()
        {
        }

        public PrintMessage(string msgConnectionName, string channel, Message message)
        {
            MSGConnectionName = msgConnectionName;
            Channel = channel;
            Message = message;
            SendingState = false;
        }

        public string MSGConnectionName { get; set; }
        public string Channel { get; set; }
        public Message Message { get; set; }
        public bool SendingState { get; set; }
}
}
