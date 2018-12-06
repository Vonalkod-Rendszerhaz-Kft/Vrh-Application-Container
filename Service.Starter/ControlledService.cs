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
            Dictionary<string, string> logData = new Dictionary<string, string>();
            logData.Add("Defined service name", Properties.ServiceName);
            logData.Add("Thread id", Thread.CurrentThread.ManagedThreadId.ToString());
            logData.Add("Check interval", Properties.CheckInterval.ToString());
            logData.Add("Check dependencies", Properties.DependenciesSemafor.ToString());
            _myOwnerPlugin.LogThis($"Started.", logData, null, LogLevel.Information, this.GetType());

            _timer = new System.Timers.Timer(properties.CheckInterval.TotalMilliseconds);
            _timer.Elapsed += CheckTime;
            _timer.AutoReset = false;
            MonitorTimerStart();
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
                Dictionary<string, string> logData = new Dictionary<string, string>();
                logData.Add("Thread id", Thread.CurrentThread.ManagedThreadId.ToString());
                logData.Add("Defined service name", Properties.ServiceName);
                try
                {
                    DateTime start = DateTime.UtcNow;
                    _myOwnerPlugin.LogThis($"Cycle started for service definition.", logData, null, LogLevel.Verbose, this.GetType());
                    var service = new ServiceController(Properties.ServiceName);
                    if (String.IsNullOrEmpty(service.ServiceName))
                    {
                        _myOwnerPlugin.LogThis($"Defined service not found on this machine!", logData, null, LogLevel.Error, this.GetType());
                        return;
                    }
                    logData.Add("Service display name", service.DisplayName);
                    string serviceStatusString = service.Status.ToString();
                    logData.Add("Service status", serviceStatusString);
                    _myOwnerPlugin.LogThis($"Service found, status checked!", logData, null, LogLevel.Verbose, this.GetType());
                    /// ha pont itt megváltozik a státusz, akkor az zavaró lehet, mert a log és a történtek nem lesznek szinkronban.!
                    if (service.Status == ServiceControllerStatus.Stopped) { StartThis(service); }

                    logData.Add("Service cycle lenght", DateTime.UtcNow.Subtract(start).ToString());
                    _myOwnerPlugin.LogThis($"Cycle finished. ", logData, null, LogLevel.Verbose, this.GetType());
                }
                catch (Exception ex)
                {
                    _myOwnerPlugin.LogThis($"Exception occured!!!", logData, ex, LogLevel.Error, this.GetType());
                }
            }
            finally {MonitorTimerStart();}
        }

        /// <summary>
        /// Újraindítja és logolja az ellenőrzési ciklus indítás időzítőt
        /// </summary>
        private void MonitorTimerStart()
        {
            _timer.Start();
            _myOwnerPlugin.LogThis($"Check interval timer restarted for service: {Properties.ServiceName}", null, null, LogLevel.Debug, this.GetType());
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
                if (disposedValue){break;}

                if (DateTime.UtcNow.Subtract(startTime) > Properties.MaxStartingWait)
                {
                    throw new Exception($"Service restart timout occured! Service: {service.ServiceName}, status: {service.Status}, timout: {Properties.MaxStartingWait}");
                }
            }
            CreateRedisSemafor(service);
            Dictionary<string, string> logData = new Dictionary<string, string>();
            logData.Add("Service name", service.ServiceName);
            _myOwnerPlugin.LogThis($"Service restart was succesfull!", logData, null, LogLevel.Information, this.GetType());
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
            var serviceDefinition = _myOwnerPlugin.Configuration.GetControlledService(service.ServiceName);
            if (serviceDefinition == null)
            {
                serviceDefinition = _myOwnerPlugin.Configuration.GetControlledService(service.DisplayName);
            }
            if (serviceDefinition != null)
            {
                redisSemaforTime = serviceDefinition.CreateRedisSemaforTime;
                semaforName = serviceDefinition.ServiceName;
            }
            semaforName = RedisSemafor(semaforName);
            logData.Add("Service name", service.ServiceName);
            logData.Add("Redis connection established", _myOwnerPlugin.RedisConnection != null ? "Yes" : "No");
            logData.Add("Redis Semafor Time", redisSemaforTime.ToString());
            logData.Add("Redis Semafor Name", semaforName);
            _myOwnerPlugin.LogThis($"Attempting to create semafor...", logData, null, LogLevel.Verbose, this.GetType());
            if (redisSemaforTime.TotalMilliseconds > 0)
            {
                var redisDb = _myOwnerPlugin.RedisConnection?.GetDatabase();
                if (redisDb != null)
                {
                    redisDb.StringSet(semaforName, DateTime.Now.ToString(), Properties.CreateRedisSemaforTime);
                    _myOwnerPlugin.LogThis($"Redis semafor is created.", logData, null, LogLevel.Information, this.GetType());
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
