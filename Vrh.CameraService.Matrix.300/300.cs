using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.ComponentModel.Composition;
using Vrh.CameraService.CameraTypeContract;

namespace Vrh.CameraService.Matrix
{
    [Export(typeof(ICameraTypeComponent))]
    [ExportMetadata("CameraType", "Matrix_300")]
    public class Matrix_300 : ICameraTypeComponent
    {
        public string Description => "Matrix 300";

        public string Name { get; set; }

        public void CreateListener(string protocolType, 
                                   string name,                                                                      
                                   bool isAllowClientConnection, 
                                   string connectionType,
                                   string connectionString,
                                   TimeSpan timeToWait,
                                   Action<string> cameraMessageAction)
        {
            IPAddress cameraListenerIP = null;
            int cameraListenerPort = 0;
            TcpListener cameraListener = null;

            try
            {
                Name = name;

                List<string> connectionParameterList = connectionString.Split(';').ToList();

                string remoteIPParameter = connectionParameterList.FirstOrDefault(x => x.IndexOf("RemoteIP") > -1);
                remoteIPParameter = remoteIPParameter.Substring(remoteIPParameter.IndexOf('=') + 1);

                string remotePortParameter = connectionParameterList.FirstOrDefault(x => x.IndexOf("RemotePort") > -1);
                remotePortParameter = remotePortParameter.Substring(remotePortParameter.IndexOf('=') + 1);

                
                if (IPAddress.TryParse(remoteIPParameter, out cameraListenerIP) && int.TryParse(remotePortParameter, out cameraListenerPort))
                {
                    cameraListener = new TcpListener(cameraListenerIP, cameraListenerPort);
                }

                cameraListener.Start();

                while (isAllowClientConnection) // <--- boolean flag to exit loop
                {
                    if (cameraListener.Pending())
                    {

                        //byte[] data = new byte[256];
                        //string responseData = string.Empty;
                        //TcpClient connectingCameraClient = null;

                        //try
                        //{
                        //    connectingCameraClient = cameraListener.AcceptTcpClient();

                        //    using (NetworkStream reader = connectingCameraClient.GetStream())
                        //    {
                        //        int bytes = reader.Read(data, 0, data.Length);
                        //        responseData = Encoding.ASCII.GetString(data, 0, bytes);
                        //        Console.WriteLine("Received new message (" + responseData.Length + " bytes):\n" + responseData);
                        //        cameraMessageAction(responseData);
                        //    }
                        //}
                        //catch (Exception ex)
                        //{
                        //    throw new Exception($"ListenerFaliure on {cameraListenerIP.ToString()}:{cameraListenerPort.ToString()} listener: {ex.Message}");
                        //}

                        TcpClient connectingCameraClient = cameraListener.AcceptTcpClient();

                        //Thread tmp_thread = new Thread(() => CameraDataReceive(connectingCameraClient, cameraMessageAction));
                        //tmp_thread.Start();

                        Task.Run(() => CameraDataReceive(connectingCameraClient, cameraMessageAction));

                    }
                    else
                    {
                        Thread.Sleep(timeToWait); //<--- timeout
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"CreateListener on {Description} failed!");
            }
            finally
            {
                cameraListener.Stop();
            }
        }

        private void CameraDataReceive(TcpClient connectingCameraClient, Action<string> cameraMessageAction)
        {
            byte[] data = new byte[256];
            string responseData = string.Empty;

            try
            {
                //connectingCameraClient = cameraListener.AcceptTcpClient();

                using (NetworkStream reader = connectingCameraClient.GetStream())
                {
                    int bytes = reader.Read(data, 0, data.Length);
                    responseData = Encoding.ASCII.GetString(data, 0, bytes);
                    Console.WriteLine("Received new message (" + responseData.Length + " bytes):\n" + responseData);
                    cameraMessageAction(responseData);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"ListenerFaliure on {Name} camera listener: {ex.Message}");
            }
        }

        public bool Connect(string protocolType, string connectionType, string connectionString, out CameraConnection cameraConnection)
        {
            cameraConnection = null;
            IPAddress cameraIP = null;
            int cameraPort = 0;

            try
            {

                List<string> connectionParameterList = connectionString.Split(';').ToList();

                string localIPParameter = connectionParameterList.FirstOrDefault(x => x.IndexOf("LocalIP") > -1);
                localIPParameter = localIPParameter.Substring(localIPParameter.IndexOf('=') + 1);

                string localPortParameter = connectionParameterList.FirstOrDefault(x => x.IndexOf("LocalPort") > -1);
                localPortParameter = localPortParameter.Substring(localPortParameter.IndexOf('=') + 1);

                if (IPAddress.TryParse(localIPParameter, out cameraIP) && int.TryParse(localPortParameter, out cameraPort))
                {
                    cameraConnection = new CameraConnection(protocolType == "DCCS" ? ProtocolTypes.DCCS : ProtocolTypes.DCCS,
                                                            cameraIP,
                                                            cameraPort);

                    // Open connection
                    cameraConnection.Client = new TcpClient();

                    cameraConnection.Client.Connect(cameraIP, cameraPort);
                }

                cameraConnection.Writer = cameraConnection.Client.GetStream();
            }
            catch (Exception ex)
            {
                //    cameraConnection = null;
                //    return false;
                throw new Exception($"Connect to {cameraIP.ToString()}:{cameraPort.ToString()} camera failed!");
            }
            
            return true;
        }

        public bool Read(CameraConnection cameraConnection, int maxTry, TimeSpan timeToWait)
        {
            bool isSuccess = false;
            int retries = 0;

            while (retries < maxTry && !isSuccess)
            {
                try
                {
                    SendToCamera("TRG\r\n", cameraConnection.Writer);
                    return true;
                }
                catch (Exception ex)
                {
                    retries++;
                    if (retries == maxTry)
                    {
                        throw ex;
                    }
                    continue;
                }
            }

            return false;
        }

        public bool GetIO(CameraConnection cameraConnection, int maxTry, TimeSpan timeToWait)
        {
            throw new NotSupportedException();
        }

        public bool SetIO(int port, bool isOn, CameraConnection cameraConnection, int maxTry, TimeSpan timeToWait)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Parancsok kiküldése a nyomtatóra streamen keresztül. (String konverzió)
        /// </summary>
        /// <param name="message">Küldendő üzenet.</param>
        /// <param name="writer">Stream a parancs küldéséhez.</param>
        private void SendToCamera(string message, NetworkStream writer)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            writer.Write(data, 0, data.Length);
            writer.Flush();
        }

        public void CloseConnection(CameraConnection cameraConnection)
        {
            // Close Connection
            cameraConnection.Writer.Close();
            cameraConnection.Client.Close();
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
