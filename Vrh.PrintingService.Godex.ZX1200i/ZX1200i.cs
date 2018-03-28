using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.ComponentModel.Composition;
using Vrh.PrintingService.PrinterTypeContract;

namespace Vrh.PrintingService.Godex.ZX1200i
{
    [Export(typeof(IPrinterTypeComponent))]
    [ExportMetadata("PrinterType", "Godex_ZX1200i")]
    public class ZX1200i : IPrinterTypeComponent
    {
        public string Description => "Godex ZX1200i";

        public bool Connect(string connectionType, string connectionString, out PrinterConnection printerConnection)
        {
            printerConnection = new PrinterConnection(connectionType != "ASECOM" ? ConnectionTypes.TCPIP : ConnectionTypes.ASECOM,
                                                      connectionString);
            // Open connection
            printerConnection.Client = new TcpClient();

            try
            {
                string printerAddress = connectionString.Substring(0, connectionString.IndexOf(':'));
                int printerPort = int.Parse(connectionString.Substring(connectionString.IndexOf(':') + 1));

                if (IPAddress.TryParse(printerAddress, out IPAddress printerIP))
                {
                    printerConnection.Client.Connect(printerIP, printerPort);
                }
            }
            catch (Exception ex)
            {
                printerConnection = null;
                return false;
            }
            // Write ZPL String to connection
            printerConnection.Writer = printerConnection.Client.GetStream();

            PrinterInitialize(printerConnection.Writer);
            return true;
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

        public bool SendPrintCommand(string labelData, PrinterConnection printerConnection, int maxTry, TimeSpan timeToWait)
        {
            bool isSuccess = false;
            int retries = 0;

            while (retries < maxTry && !isSuccess)
            {
                try
                {
                    if (IsError(printerConnection.Writer))
                    {
                        retries++;
                        Thread.Sleep(timeToWait);
                        continue;
                    }

                    SendToPrinter(labelData, printerConnection.Writer);
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

        public bool VerifyPrinterState(PrinterConnection printerConnection, int maxTry, TimeSpan timeToWait)
        {
            bool isSuccess = false;
            int retries = 0;

            while (retries < maxTry || !isSuccess)
            {
                try
                {
                    if (IsError(printerConnection.Writer))
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
        /// Nyomtató státuszának lekérdezése, hiba detektálás céljából.
        /// </summary>
        /// <param name="writer">Stream a parancsok kiküldéséhez.</param>
        /// <returns>Van-e hiba a nyomtatóban.</returns>
        private bool IsError(NetworkStream writer)
        {
            byte[] data = new byte[256];
            string responseData = string.Empty;

            this.SendToPrinter("~S,CHECK\r", writer);
            //int bytes = writer.Read(data, 0, data.Length);
            //responseData = Encoding.ASCII.GetString(data, 0, bytes);
            //bool isError = (responseData == string.Empty) || !responseData.Contains("00");

            //return isError;

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

        public void CloseConnection(PrinterConnection printerConnection)
        {
            // Close Connection
            printerConnection.Writer.Close();
            printerConnection.Client.Close();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {

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
