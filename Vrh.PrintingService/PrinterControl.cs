using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.IO;
using Vrh.PrintingService.MessageTypes;
using Vrh.PrintingService.PrinterTypeContract;

namespace Vrh.PrintingService
{
    public class PrinterControl : IDisposable
    {
        public Printer printer;
        System.Timers.Timer timer;
        TimeSpan interval;
        TimeSpan timeToWait;
        string PathOfLabels;
        int maxTry;
        public ConcurrentQueue<PrintMessage> messageCache;
        Stopwatch stopwatch = new Stopwatch();
        int counter;

        PrinterTypeImporter pti = null;

        public PrinterControl(Printer printer, TimeSpan messageTimeout, TimeSpan interval, TimeSpan timeToWait, int maxTry, string pathOfLabels)
        {
            //MEF Printer típusok betöltése
            pti = new PrinterTypeImporter();
            pti.DoImport();

            messageCache = new ConcurrentQueue<PrintMessage>();
            MessageTimeout = messageTimeout;
            this.interval = interval;
            this.printer = printer;
            this.maxTry = maxTry;
            this.timeToWait = timeToWait;
            this.PathOfLabels = pathOfLabels;
            SetTimer();
            stopwatch.Start();
        }

        public TimeSpan MessageTimeout { get; }

        /// <summary>
        /// Set the timers preferences from the log4proDBcontext
        /// </summary>
        private void SetTimer()
        {
            this.timer = new System.Timers.Timer(interval.TotalMilliseconds);
            this.timer.Elapsed += this.ProcessPrintMessages;
        }

        /// <summary>
        /// Kezeletlen címkék feldolgozása
        /// </summary>
        /// <param name="sender">Esemény kiváltója</param>
        /// <param name="e">Event parameter</param>
        private void ProcessPrintMessages(object sender, ElapsedEventArgs e)
        {

            counter++;
            //Console.WriteLine(counter / stopwatch.Elapsed.TotalSeconds);

            timer.Stop();
            try
            {
                if (messageCache.Count != 0)
                {
                    SendPrintMessages();
                }
            }
            finally
            {
                timer.Start();
            }
        }

        private void SendPrintMessages()
        {
            PrintMessage printMessage;
            while (messageCache.TryDequeue(out printMessage))
            {
                try
                {
                    Console.WriteLine($"1:{printMessage.PrinterName}");

                    bool successSend = true;
                    bool successPrint = false;

                    if (!pti.Connect(printer.Type, printer.ConnectionType, printer.ConnectionString, out PrinterConnection printerConnection))
                    {
                        SetPrintMessagePreferences(printMessage, false);
                        return;
                    }

                    string messageData = String.Empty;

                    if (!string.IsNullOrEmpty(printMessage.Message.LabelDefinitionName))
                    {
                        //Label nyomtatás
                        messageData = ComposeLabel(printMessage);
                    }
                    else
                    {
                        //Nyomtató parancs küldés
                        messageData = printMessage.Message.Stream;
                    }

                    successSend = pti.SendPrintCommand(printer.Type, messageData, printerConnection, maxTry, timeToWait);
                    Thread.Sleep(3000);
                    if (successSend)
                    {
                        successPrint = pti.VerifyPrinterState(printer.Type, printerConnection, maxTry, timeToWait);
                    }

                    SetPrintMessagePreferences(printMessage, successPrint);

                    Console.WriteLine($"2:{printMessage.PrinterName}");

                    pti.CloseConnection(printer.Type, printerConnection);                    
                }
                catch (Exception ex)
                {
                    printMessage.Exception = ex;
                }
                finally
                {
                    printMessage.Semaphore.Set();
                }
            }
        }

        internal void StartTimer()
        {
            this.timer.Start();
        }

        /// <summary>
        /// Címke mezőinek beállítása sikeres vagy sikertelen nyomtatást követően (PrintStatus és PrintDate)
        /// </summary>
        /// <param name="isSuccess">A nyomtatás sikeres volt-e</param>
        private void SetPrintMessagePreferences(PrintMessage printMessage, bool isSuccess)
        {
            if (isSuccess)
            {
                printMessage.SendingState = true;
            }
            else
            {
                printMessage.SendingState = false;
            }
        }

        /// <summary>
        /// Címkeformátum feltöltése a kiküldendő címke adatokkal
        /// </summary>
        /// <param name="printMessage">Címke objektum</param>
        /// <returns>Címkeformátum az adatokkal feltöltve</returns>
        private string ComposeLabel(PrintMessage printMessage)
        {
            LabelDefinition labelDefinition = this.printer.LabelDefinitions.FirstOrDefault(x => x.Name == printMessage.Message.LabelDefinitionName);
            if (labelDefinition == null)
            {
                return String.Empty;
            }

            string labelData = File.ReadAllText(Path.Combine(PathOfLabels, labelDefinition.Name));

            foreach (KeyValuePair<string, string> entry in printMessage.Message.DataList)
            {
                string startNS = labelDefinition.Nameseparator.Substring(0);
                string endNS = labelDefinition.Nameseparator.Length == 2 ? labelDefinition.Nameseparator.Substring(1, 1) : labelDefinition.Nameseparator.Substring(0);
                labelData = labelData.Replace(String.Format("{0}{1}{2}", startNS, entry.Key, endNS), entry.Value);
            }

            return labelData;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    timer.Stop();
                    timer.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~PrinterControl() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
