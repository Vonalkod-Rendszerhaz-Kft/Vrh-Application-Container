using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vrh.ApplicationContainer;
using Vrh.Logger;

namespace Service.Starter
{
    /// <summary>
    /// Egy kontrolált windows service-t reprezentáló objektum
    /// </summary>
    internal class ControlledService : IDisposable
    {
        /// <summary>
        /// Construktor
        /// </summary>
        /// <param name="properties"></param>
        public ControlledService(ServiceProperties properties, ServiceStarterPlugin pluginReference)
        {
            _myOwnerPlugin = pluginReference;            
            Properties = properties;
            _timer = new System.Timers.Timer(properties.CheckInterval.TotalMilliseconds);
            _timer.Elapsed += CheckTime;
            _timer.AutoReset = false;
            _timer.Start();
            Dictionary<string, string> logData = new Dictionary<string, string>();
            logData.Add("Interval", Properties.CheckInterval.ToString());
            logData.Add("Check dependencies", Properties.DependenciesSemafor.ToString());
            _myOwnerPlugin.LogThis($"{Properties.ServiceName} is under controll now.", logData, null, LogLevel.Information, this.GetType());
        }

        /// <summary>
        /// A kontrolált service kontroljának alapvető tulajdonságai
        /// </summary>
        public ServiceProperties Properties { get; private set; }

        /// <summary>
        /// Ellenőrzi a Service-futását.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckTime(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                DateTime start = DateTime.UtcNow;
                Dictionary<string, string> logData = new Dictionary<string, string>();
                try
                {                    
                    logData.Add("Thread id", Thread.CurrentThread.ManagedThreadId.ToString());
                    logData.Add("Defined service name", Properties.ServiceName);
                    _myOwnerPlugin.LogThis($"Checking...", logData, null, LogLevel.Verbose, this.GetType());
                    var service = new ServiceController(Properties.ServiceName);
                    if (String.IsNullOrEmpty(service.ServiceName))
                    {
                        _myOwnerPlugin.LogThis("Service not found in this machine!", logData, null, LogLevel.Error, this.GetType());
                        return;
                    }
                    logData.Add("Service name", service.ServiceName);
                    logData.Add("Service display name", service.DisplayName);
                    logData.Add("Service status", service.Status.ToString());                                        
                    _myOwnerPlugin.LogThis("Service found", logData, null, LogLevel.Debug, this.GetType());
                    CheckServiceStatus(service);
                    logData.Add("Full check time", DateTime.UtcNow.Subtract(start).ToString());
                    _myOwnerPlugin.LogThis("Is checked.", logData, null, LogLevel.Verbose, this.GetType());
                }
                catch (Exception ex)
                {
                    _myOwnerPlugin.LogThis("Exception occured!!!", logData, ex, LogLevel.Error, this.GetType());
                }
            }
            finally
            {
                _timer.Start();
                _myOwnerPlugin.LogThis($"{Properties.ServiceName} timer is restarted...", null, null, LogLevel.Debug, this.GetType());
            }
        }

        /// <summary>
        /// Ellenőrzi a service állapotát
        /// </summary>
        /// <param name="service">Az elelnőrizebndő service</param>
        private void CheckServiceStatus(ServiceController service)
        {
            Dictionary<string, string> logData = new Dictionary<string, string>();
            logData.Add("Service name", service.ServiceName);
            logData.Add("Service display name", service.DisplayName);
            logData.Add("Service current status", service.Status.ToString());
            _myOwnerPlugin.LogThis($"Check this service status: {service.DisplayName}", logData, null, LogLevel.Verbose, this.GetType());
            switch (service.Status)
            {
                case ServiceControllerStatus.ContinuePending:
                case ServiceControllerStatus.PausePending:
                case ServiceControllerStatus.StartPending:
                case ServiceControllerStatus.StopPending:
                    _myOwnerPlugin.LogThis("Service is Pending status!", logData, null, LogLevel.Warning, this.GetType());
                    break;
                case ServiceControllerStatus.Paused:
                    _myOwnerPlugin.LogThis("Service is Paused status!", logData, null, LogLevel.Warning, this.GetType());
                    break;
                case ServiceControllerStatus.Running:
                    _myOwnerPlugin.LogThis("OK! Service is Running status.", logData, null, LogLevel.Information, this.GetType());
                    break;
                case ServiceControllerStatus.Stopped:
                    _myOwnerPlugin.LogThis("Service is Stopped status! Restarting...", logData, null, LogLevel.Error, this.GetType());
                    StartThis(service);
                    break;
            }
        }

        /// <summary>
        /// Eloindítja a service-t, és megvárja míg elindul
        /// </summary>
        /// <param name="service">Az elindítandó service</param>
        private void StartThis(ServiceController service)
        {
            CreateRedisSemafor(service);
            DateTime startTime = DateTime.UtcNow;
            service.Start();
            while(service.Status != ServiceControllerStatus.Running)
            {
                Thread.Sleep(100);
                service.Refresh();
                if (disposedValue)
                {
                    break;
                }
                if (DateTime.UtcNow.Subtract(startTime) > Properties.MaxStartingWait)
                {
                    throw new Exception($"Timout occured! Started service: {service.DisplayName}; Service status: {service.Status}; Used timout: {Properties.MaxStartingWait}");
                }
            }
            CreateRedisSemafor(service);
            _myOwnerPlugin.LogThis($"Service starting succesfull: { service.ServiceName }", null, null, LogLevel.Information, this.GetType());
        }

        /// <summary>
        /// Beteszi a Redisbe a service indítás semaforját
        /// </summary>
        /// <param name="service"></param>
        private void CreateRedisSemafor(ServiceController service)
        {
            Dictionary<string, string> logData = new Dictionary<string, string>();
            TimeSpan redisSemaforTime = _myOwnerPlugin.Configuration.DefaultRedisSemaforTime;
            string semaforName = service.ServiceName;
            var serviceDefination = _myOwnerPlugin.Configuration.GetControlledService(service.ServiceName);
            if (serviceDefination == null)
            {
                serviceDefination = _myOwnerPlugin.Configuration.GetControlledService(service.DisplayName);
            }
            if (serviceDefination != null)
            {
                redisSemaforTime = serviceDefination.CreateRedisSemaforTime;
                semaforName = serviceDefination.ServiceName;
            }
            semaforName = RedisSemafor(semaforName);
            logData.Add("Redis defined?", _myOwnerPlugin.RedisConnection != null ? "Yes" : "No");
            logData.Add("Redis Semafor Time", redisSemaforTime.ToString());
            logData.Add("Redis Semafor Name", semaforName);
            _myOwnerPlugin.LogThis("Is Redis Semafore Need?", logData, null, LogLevel.Verbose, this.GetType());
            if (redisSemaforTime.TotalMilliseconds > 0)
            {
                var redisDb = _myOwnerPlugin.RedisConnection?.GetDatabase();
                if (redisDb != null)
                {
                    redisDb.StringSet(semaforName, DateTime.Now.ToString(), Properties.CreateRedisSemaforTime);
                    _myOwnerPlugin.LogThis("Redis semafor is created.", logData, null, LogLevel.Information, this.GetType());
                }
            }
            if (Properties.DependenciesSemafor && service.ServicesDependedOn.Length > 0)
            {
                foreach (var dService in service.ServicesDependedOn)
                {
                    CreateRedisSemafor(dService);
                }
            }
        }

        /// <summary>
        /// Redisen használt semafor kulcs
        /// </summary>
        private string RedisSemafor(string serviceName)
        {
            return $"Service.Starter.Semafor.{serviceName}";
        }

        /// <summary>
        /// Timer, amely "üt", mikor ellenőrizni kell a szervice-t
        /// </summary>
        private System.Timers.Timer _timer = null;

        /// <summary>
        /// 
        /// </summary>
        private ServiceStarterPlugin _myOwnerPlugin = null;

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _timer.Stop();
                    _timer.Elapsed -= CheckTime;
                    _timer.Dispose();
                    _timer = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ControlledService() {
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

    /// <summary>
    /// Egy kontrolált service alapvető tulajdonásgait írja le
    /// </summary>
    internal class ServiceProperties
    {
        /// <summary>
        /// Windows service neve (name property)
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// Kell-e semafort rakni azokra a servizekre, amelyektől ez  aservice függ, amikor elindítja ezt a service-t
        /// </summary>
        public bool DependenciesSemafor { get; set; }

        /// <summary>
        /// Az ellenőrzés intervalluma
        /// </summary>
        public TimeSpan CheckInterval { get; set; }

        /// <summary>
        /// Meddig várunk egy service elindulásának megtörténtére
        /// </summary>
        public TimeSpan MaxStartingWait { get; set; } = new TimeSpan(0, 1, 0);

        /// <summary>
        /// Mennyi ideig kell a Redis semafort a redisbe tenni, ha nulla, akkor nem kell használni
        /// </summary>
        public TimeSpan CreateRedisSemaforTime { get; set; } = new TimeSpan(0);
    }
}
