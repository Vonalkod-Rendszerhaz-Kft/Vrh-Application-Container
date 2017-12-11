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

namespace Vrh.ApplicationContainer.ConsoleHost
{
    class Program
    {
        static void Main(string[] args)
        {
            //var mq = new MessageQueue(@".\private$\Test");
            //mq.Formatter = new ActiveXMessageFormatter();
            ////MessageQueue.Exists();
            ////mq.Peek()

            
            //Message msg = new Message();
            
            ////msg.ResponseQueue = this.responseQueue;
            //msg.Formatter = new ActiveXMessageFormatter();
            //msg.Label = "Test";
            //msg.BodyStream = new MemoryStream(Encoding.UTF8.GetBytes("AS01#$IVPC@1"));
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
