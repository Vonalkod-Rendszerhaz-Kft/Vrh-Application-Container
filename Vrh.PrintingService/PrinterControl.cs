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

        public Dictionary<string, string> PrinterStates = new Dictionary<string, string>()
        {
           { "00", "Ready"},
           { "01 ", "Media Empty or Media Jam"},
           { "02", "Media Empty or Media Jam"},
           { "03", "Ribbon Empty"},
           { "04", "Printhead Up ( Open )"},
           { "05", "Rewinder Full"},
           { "06", "File System Full"},
           { "07", "Filename Not Found"},
           { "08", "Duplicate Name"},
           { "09", "Syntax error"},
           { "10", "Cutter JAM"},
           { "11", "Extended Menory Not Found"},
           { "20", "Pause"},
           { "21", "In Setting Mode"},
           { "22", "In Keyboard Mode"},
           { "50", "Printer is Printing"},
           { "60", "Data in Process"},
        };

        public PrinterControl(Printer printer, TimeSpan interval, TimeSpan timeToWait, int maxTry, string pathOfLabels)
        {
            //MEF Printer típusok betöltése
            pti = new PrinterTypeImporter();
            pti.DoImport();

            messageCache = new ConcurrentQueue<PrintMessage>();
            this.interval = interval;
            this.printer = printer;
            this.maxTry = maxTry;
            this.timeToWait = timeToWait;
            this.PathOfLabels = pathOfLabels;
            SetTimer();
            stopwatch.Start();
        }

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
            Console.WriteLine(counter / stopwatch.Elapsed.TotalSeconds);

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
                Console.WriteLine($"1:{printMessage.MSGConnectionName}");

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

                Console.WriteLine($"2:{printMessage.MSGConnectionName}");

                pti.CloseConnection(printer.Type, printerConnection);
            }
        }

        //private void SendPrintMessages()
        //{
        //    PrintMessage printMessage;
        //    while (messageCache.TryDequeue(out printMessage))
        //    {
        //        Console.WriteLine($"1:{printMessage.MSGConnectionName}");

        //        bool successSend = true;
        //        bool successPrint = false;

        //        if (!ConnectToPrinter(printer.ConnectionType, printer.ConnectionString, out TcpClient client, out NetworkStream writer))
        //        {
        //            SetPrintMessagePreferences(printMessage, false);
        //            return;
        //        }

        //        PrinterInitialize(writer);

        //        string messageData = String.Empty;

        //        if (!string.IsNullOrEmpty(printMessage.Message.LabelDefinitionName))
        //        {
        //            //Label nyomtatás
        //            messageData = ComposeLabel(printMessage);
        //        }
        //        else
        //        {
        //            //Nyomtató parancs küldés
        //            messageData = printMessage.Message.Stream;
        //        }

        //        successSend = SendPrintCommand(messageData, writer);
        //        Thread.Sleep(3000);
        //        if (successSend)
        //        {
        //            successPrint = VerifyPrinting(writer);
        //        }

        //        SetPrintMessagePreferences(printMessage, successPrint);

        //        Console.WriteLine($"2:{printMessage.MSGConnectionName}");

        //        CloseConnection(client, writer);
        //    }
        //}

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
        /// Nyomtatási parancs kiküldése utáni státuszellenőrzés.
        /// </summary>
        /// <param name="writer">Stream a parancsok kiküldéséhez.</param>
        /// <returns>Nyomtatás sikeressége</returns>
        private bool VerifyPrinting(NetworkStream writer)
        {
            bool isSuccess = false;
            int retries = 0;

            while (retries < maxTry || !isSuccess)
            {
                try
                {
                    if (IsError(writer))
                    {
                        retries++;
                        Thread.Sleep(timeToWait);
                        continue;
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    retries++;
                    continue;
                }
            }

            return false;
        }

        /// <summary>
        /// Nyomtatási parancs kiküldése a nyomtatónak.
        /// </summary>
        /// <param name="labelData">Címkeformátum a nyomtatandó adatokkal</param>
        /// <param name="writer">Stream a parancsok küldéséhez.</param>
        /// <returns>Parancs kiküldésének sikeressége</returns>
        private bool SendPrintCommand(string labelData, NetworkStream writer)
        {
            bool isSuccess = false;
            int retries = 0;

            while (retries < maxTry && !isSuccess)
            {
                try
                {
                    if (IsError(writer))
                    {
                        retries++;
                        Thread.Sleep(timeToWait);
                        continue;
                    }

                    SendToPrinter(labelData, writer);
                    return true;
                }
                catch (Exception ex)
                {
                    retries++;
                    continue;
                }
            }

            return false;
        }

        /// <summary>
        /// Parancsok kiküldése a nyomtatóra streamen keresztül. (String konverzió)
        /// </summary>
        /// <param name="message">Küldendő üzenet.</param>
        /// <param name="writer">Stream a parancs küldéséhez.</param>
        private void SendToPrinter(string message, NetworkStream writer)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            writer.Write(data, 0, data.Length);
            writer.Flush();
        }

        /// <summary>
        /// Tcp socket és stream lezárása.
        /// </summary>
        /// <param name="client">Tcp socket</param>
        /// <param name="writer">Tcp socketen levő stream.</param>
        private void CloseConnection(TcpClient client, NetworkStream writer)
        {
            // Close Connection
            writer.Close();
            client.Close();
        }

        /// <summary>
        /// Kapcsolódás a nyomtatóhoz.
        /// </summary>
        /// <param name="printerAddress">Nyomtató IP címe</param>
        /// <param name="client">Tcp socket - kimenő paraméter</param>
        /// <param name="writer">Stream a Tcp socketen - kimenő paraméter</param>
        private bool ConnectToPrinter(string connectionType, string connectionString, out TcpClient client, out NetworkStream writer)
        {
            // Open connection
            client = new TcpClient();

            try
            {
                string printerAddress = connectionString.Substring(0, connectionString.IndexOf(':'));
                int printerPort = int.Parse(connectionString.Substring(connectionString.IndexOf(':') + 1));

                if (IPAddress.TryParse(printerAddress, out IPAddress printerIP))
                {
                    client.Connect(printerIP, printerPort);
                }
            }
                catch (Exception ex)
            {
                writer = null;
                return false;
            }
            // Write ZPL String to connection
            writer = client.GetStream();
            return true;

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

        /// <summary>
        /// Nyomtató alap beállítása.
        /// </summary>
        /// <param name="writer">Stream a parancsok kiküldéséhez.</param>
        /// <returns>Van-e hiba a nyomtatóban.</returns>
        private bool PrinterInitialize(NetworkStream writer)
        {
            //byte[] data = new byte[256];
            //string responseData = string.Empty;

            this.SendToPrinter("^XSET,IMMEDIATE,1\r", writer);
            //int bytes = writer.Read(data, 0, data.Length);
            //responseData = Encoding.ASCII.GetString(data, 0, bytes);
            //bool isError = (responseData == string.Empty) || responseData.Contains(" 1 ");

            return true;
        }

        /// <summary>
        /// Nyomtató státuszának lekérdezése, hiba detektálás céljából.
        /// </summary>
        /// <param name="writer">Stream a parancsok kiküldéséhez.</param>
        /// <returns>Van-e hiba a nyomtatóban.</returns>
        private bool IsError(NetworkStream writer)
        {
            byte[] data = new byte[256];
            string responseData = string.Empty;

            this.SendToPrinter("~S,CHECK\r", writer);
            int bytes = writer.Read(data, 0, data.Length);
            responseData = Encoding.ASCII.GetString(data, 0, bytes);
            bool isError = (responseData == string.Empty) || !responseData.Contains("00");

            return isError;
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
