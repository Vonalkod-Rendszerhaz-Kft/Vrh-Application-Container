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
using Vrh.CameraService.MessageTypes;
using Vrh.CameraService.CameraTypeContract;
using Vrh.EventHub.Core;
using Vrh.EventHub.Protocols.RedisPubSub;

namespace Vrh.CameraService
{
    public class CameraControl : IDisposable
    {
        public Camera camera = null;
        
        TimeSpan interval;
        TimeSpan timeToWait;
        int maxTry;
        Stopwatch stopwatch = new Stopwatch();
        int counter;

        CameraTypeImporter pti = null;
        Action<string> cameraMessageAction;

        static bool IsAllowClientConnection { get; set; }
        static string LabelData { get; set; }
        static bool IOState { get; set; }
        static bool SendingState { get; set; }
        static Exception Exception { get; set; }
        static AutoResetEvent Semaphore { get; set; }

        public CameraControl(Camera camera, TimeSpan messageTimeout, TimeSpan interval, TimeSpan timeToWait, int maxTry)
        {
            cameraMessageAction = ReceiveCameraMessage;

            //MEF Printer típusok betöltése
            pti = new CameraTypeImporter();
            pti.DoImport();
            
            MessageTimeout = messageTimeout;
            this.interval = interval;
            this.camera = camera;
            this.maxTry = maxTry;
            this.timeToWait = timeToWait;
            stopwatch.Start();

            IsAllowClientConnection = true;

            Task.Run(() => pti.CreateListener(camera.Type,
                               camera.Name,
                               camera.Protocol,
                               IsAllowClientConnection,
                               camera.ConnectionType,
                               camera.ConnectionString,
                               this.timeToWait,
                               cameraMessageAction));

            EventHubCore.RegisterHandler<RedisPubSubChannel, ReadMessage, LabelDataMessageResult>(camera.Name, Read);
            EventHubCore.RegisterHandler<RedisPubSubChannel, GetIOMessage, IOChangeMessageResult>(camera.Name, GetIO);
            EventHubCore.RegisterHandler<RedisPubSubChannel, SetIOMessage, CommonMessageResult>(camera.Name, SetIO);

            #region Debug camera call
            ////Debug idejére
            ////Callhoz kérés összerakása

            //ReadMessage rm = new ReadMessage();
            //try
            //{
            //    LabelDataMessageResult ldmr = EventHubCore.Call<RedisPubSubChannel, ReadMessage, LabelDataMessageResult>(camera.Name, rm, null);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"Exception:{ex.Message}");
            //}
            #endregion
        }

        public TimeSpan MessageTimeout { get; }

        private void SendCameraRead(ReadMessage readMessage)
        {
            try
            {
                Console.WriteLine($"1:{camera.Name} got Read command");

                if (!pti.Connect(camera.Type, camera.Protocol, camera.ConnectionType, camera.ConnectionString, out CameraConnection cameraConnection))
                {
                    return;
                }

                SendingState = pti.Read(camera.Type, cameraConnection, maxTry, timeToWait);


                Console.WriteLine($"2:{camera.Name} Read successfull");

                pti.CloseConnection(camera.Type, cameraConnection);                    
            }
            catch (Exception ex)
            {
                Exception = ex;
            }
            //finally
            //{
            //    Semaphore.Set();
            //}
        }

        private void SendCameraGetIO(GetIOMessage getIOMessage)
        {
            try
            {
                Console.WriteLine($"1:{camera.Name} got GetIO command");

                if (!pti.Connect(camera.Type, camera.Protocol, camera.ConnectionType, camera.ConnectionString, out CameraConnection cameraConnection))
                {
                    return;
                }

                SendingState = pti.GetIO(camera.Type, cameraConnection, maxTry, timeToWait);

                Console.WriteLine($"2:{camera.Name} GetIO successfull");

                pti.CloseConnection(camera.Type, cameraConnection);
            }
            catch (Exception ex)
            {
                Exception = ex;
            }
            //finally
            //{
            //    Semaphore.Set();
            //}
        }

        private void SendCameraSetIO(SetIOMessage setIOMessage)
        {
            try
            {
                Console.WriteLine($"1:{camera.Name} got SetIO command");

                if (!pti.Connect(camera.Type, camera.Protocol, camera.ConnectionType, camera.ConnectionString, out CameraConnection cameraConnection))
                {
                    return;
                }

                SendingState = pti.SetIO((int)setIOMessage.Bulb, setIOMessage.BulbTurneOn, camera.Type, cameraConnection, maxTry, timeToWait);

                Console.WriteLine($"2:{camera.Name} SetIO successfull");

                pti.CloseConnection(camera.Type, cameraConnection);
            }
            catch (Exception ex)
            {
                Exception = ex;
            }
            //finally
            //{
            //    Semaphore.Set();
            //}
        }

        /// <summary>
        /// Címkeformátum feltöltése a kiküldendő címke adatokkal
        /// </summary>
        /// <param name="cameraMessage">Címke objektum</param>
        /// <returns>Címkeformátum az adatokkal feltöltve</returns>
        private string ComposeLabel(ReadMessage cameraMessage)
        {
            

            return "";
        }

        private void ReceiveCameraMessage(string message)
        {
            //
            try
            {
                if (message.IndexOf("DI; ") > -1 || message.IndexOf("COM; ") > -1)
                {
                    if (message.IndexOf("DI; ") > -1)
                    {
                        string stateString = message.Substring(message.IndexOf("DI; ") + 4);

                        if (stateString.Substring(0, 2) == "ON")
                        {
                            IOState = true;

                            Console.WriteLine($"[{camera.Name}] IOState changed to: ON");
                        }
                        else if (stateString.Substring(0, 3) == "OFF")
                        {
                            IOState = false;

                            Console.WriteLine($"[{camera.Name}] IOState changed to: OFF");
                        }
                        else if (stateString.Substring(0, 8) == "STATUSON")
                        {
                            IOChangeMessageResult iocmr = new IOChangeMessageResult(true);
                            EventHubCore.SendAsync<RedisPubSubChannel, IOChangeMessageResult>(camera.Name, iocmr);

                            Console.WriteLine($"[{camera.Name}] IOState: ON");
                            return;
                        }
                        else if (stateString.Substring(0, 9) == "STATUSOFF")
                        {
                            IOChangeMessageResult iocmr = new IOChangeMessageResult(false);
                            EventHubCore.SendAsync<RedisPubSubChannel, IOChangeMessageResult>(camera.Name, iocmr);

                            Console.WriteLine($"[{camera.Name}] IOState: OFF");
                            return;
                        }

                    }
                    else if (message.IndexOf("COM; ") > -1)
                    {
                        // ManualData
                        LabelData = message.Substring(message.IndexOf("COM; ") + 5);
                        ManualLabelDataMessageResult mldmr = new ManualLabelDataMessageResult(LabelData);
                        EventHubCore.SendAsync<RedisPubSubChannel, ManualLabelDataMessageResult>(camera.Name, mldmr);

                        Console.WriteLine($"[{camera.Name}] Manual label data: {LabelData}");
                        return;
                    }
                }
                else
                {
                    //AutoData
                    //Label Message
                    LabelData = message;

                    Console.WriteLine($"[{camera.Name}] Auto label data: {LabelData}");
                }
            }
            catch (Exception ex)
            {
                Exception = ex;
            }
            finally
            {
                Semaphore.Set();
            }
        }

        private static void ClearCameraState()
        {
            LabelData = string.Empty;
            IOState = false;
            SendingState = false;
            Exception = null;
            Semaphore = new AutoResetEvent(false);
        }

        public Response<LabelDataMessageResult> Read(Request<ReadMessage, LabelDataMessageResult> message)
        {
            Response<LabelDataMessageResult> ldmr = message.MyResponse;

            try
            {
                ClearCameraState();

                SendCameraRead(message.RequestContent);
                Semaphore.WaitOne(MessageTimeout);

                if (Exception != null)
                {
                    throw Exception;
                }

                if (!SendingState)
                {
                    throw new Exception("Camera timeout!");
                }

                ldmr.ResponseContent = new LabelDataMessageResult(LabelData);
            }
            catch (Exception ex)
            {
                ldmr.Exception = ex;
            }

            return ldmr;
        }

        public Response<IOChangeMessageResult> GetIO(Request<GetIOMessage, IOChangeMessageResult> message)
        {
            Response<IOChangeMessageResult> cmr = message.MyResponse;

            try
            {
                ClearCameraState();

                SendCameraGetIO(message.RequestContent);
                Semaphore.WaitOne(MessageTimeout);

                if (Exception != null)
                {
                    throw Exception;
                }

                if (!SendingState)
                {
                    throw new Exception("Camera timeout!");
                }

                cmr.ResponseContent = new IOChangeMessageResult(IOState);
            }
            catch (Exception ex)
            {
                cmr.Exception = ex;
            }

            return cmr;
        }

        public Response<CommonMessageResult> SetIO(Request<SetIOMessage, CommonMessageResult> message)
        {
            Response<CommonMessageResult> cmr = message.MyResponse;

            try
            {
                ClearCameraState();

                SendCameraSetIO(message.RequestContent);
                Semaphore.WaitOne(MessageTimeout);

                if (Exception != null)
                {
                    throw Exception;
                }

                if (!SendingState)
                {
                    throw new Exception("Camera timeout!");
                }

                cmr.ResponseContent = new CommonMessageResult();
            }
            catch (Exception ex)
            {
                cmr.Exception = ex;
            }

            return cmr;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    EventHubCore.DropChannel<RedisPubSubChannel>(camera.Name);
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
