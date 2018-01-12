using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vrh.ApplicationContainer;
using Vrh.Logger;
using System.Messaging;
using System.IO;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;

namespace Vrh.ApplicationContainer.ConsoleHost
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!Debugger.IsAttached)
            {
                Console.WriteLine("Attach the debugger now if need and press a key here to continue...");
                Console.ReadLine();
            }

            var mq = new MessageQueue(@".\private$\Test");
            mq.Formatter = new ActiveXMessageFormatter();
            //var mq = new MessageQueue(@".\private$\Test");
            //mq.Formatter = new ActiveXMessageFormatter();
            ////MessageQueue.Exists();
            ////mq.Peek()


            Message msg = new Message();

            //msg.ResponseQueue = this.responseQueue;
            msg.Formatter = new ActiveXMessageFormatter();
            msg.Label = "Test";
            //msg.BodyStream = new MemoryStream(Encoding.Unicode.GetBytes("AS01#$IVPPC#$IVSRC=CP;GPN=gpn;CPN=cpn;LOT=lot;SUSN=susn;ITSN=itsn;qty=1;DATETIME=2017.12.22 12:00:00"));
            //msg.BodyStream = new MemoryStream(Encoding.Unicode.GetBytes("AS01#$IVPPL#$IVSRC=LJS;WO=wo;FYON=fyon;DATETIME=2017.12.22 12:00:00"));
            msg.BodyStream = new MemoryStream(Encoding.Unicode.GetBytes("AS01#$IVPC#$Count=1"));
            
            //msg.BodyStream = new MemoryStream(Encoding.UTF8.GetBytes("IVPPL"));
            //msg.BodyStream = new MemoryStream(Encoding.UTF8.GetBytes("IVPPL#$PRLINE=AS01;ivsrc=LJS"));
            //msg.BodyStream = new MemoryStream(Encoding.Unicode.GetBytes("AS01#$IVPPL#$LJS;100285467;636774853;2;2017-05-31 12:27:37"));
            //msg.UseDeadLetterQueue = true;
            //mq.MessageReadPropertyFilter.CorrelationId = true;
            ////msg.CorrelationId = "1".PadLeft(20, '0');
            ////msg.TimeToBeReceived = TimeSpan.FromMinutes(settings.MSMQReceiveTimout);
            //mq.Send(msg);
            ////this.collerationId = msg.Id;

            ApplicationContainer appC = null;
            try
            {
                appC = new ApplicationContainer();
            }
            catch (FatalException ex)
            {
                VrhLogger.Log(ex, typeof(Program), LogLevel.Fatal);
            }
            Thread.Sleep(3000);                       
            Console.WriteLine("Application container Started.");
            var ck = Console.ReadKey();
            if (ck.Key == ConsoleKey.T)
            {
                TcpClient c = new TcpClient("127.0.0.1", 3301);
                foreach (var item in "AS01#$ivppc$#IvSrc=CP;gpn=1;cpn=1;lot=1;susn=1;itsn=1;qty=1;DateTime=1;\r\n")
                {
                    c.Client.Send(new byte[1] { Convert.ToByte(item) });
                    Thread.Sleep(100);
                }
                    
            }

            Console.WriteLine("Press enter to Dispose");
            Console.ReadLine();
            if (appC !=  null)
            {
                appC.Dispose();
            }
            Thread.Sleep(3000);
            Console.WriteLine("Application container disposed.");
            Console.WriteLine("Press enter to Exit");
            Console.ReadLine();
        }
    }

    public class Test
    {
        public string StrField { get; set; }

        public int IntField { get; set; }

        public bool BoolField { get; set; }

        public override string ToString()
        {
            return String.Format("{0}; {1}; {2}", StrField, IntField, BoolField);
        }
    }
}
