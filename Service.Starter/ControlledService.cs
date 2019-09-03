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

            double _modifyer = (double)(120 - DateTime.Now.Ticks % 40) / 100;
            _timer = new System.Timers.Timer(properties.CheckInterval.TotalMilliseconds * _modifyer);
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
            Dictionary<string, string> logData = new Dictionary<string, string>();
            _myOwnerPlugin.LogThis($"Service status check cycle started for service {Properties.ServiceName}.", null, null, LogLevel.Verbose, this.GetType());
            PluginStateEnum s = _myOwnerPlugin.Status.State;
            if (s != PluginStateEnum.Running)
            {
                _myOwnerPlugin.LogThis($"Plugin instance state is not 'Running' for service {Properties.ServiceName}, check skipped.", null, null, LogLevel.Verbose, this.GetType());
                return;
            }

            try
            {
                logData.Add("Thread id", Thread.CurrentThread.ManagedThreadId.ToString());
                try
                {
                    DateTime start = DateTime.UtcNow;
                    var service = new ServiceController(Properties.ServiceName);
                    if (String.IsNullOrEmpty(service.ServiceName))
                    {
                        _myOwnerPlugin.LogThis($"Defined service {service.ServiceName} not found on this machine!", null, null, LogLevel.Error, this.GetType());
                        return;
                    }
                    _myOwnerPlugin.LogThis($"Service {service.ServiceName} found, status checked. Current status: {service.Status}.!", null, null, LogLevel.Verbose, this.GetType());
                    /// ha pont itt megváltozik a státusz, akkor az zavaró lehet, mert a log és a történtek nem lesznek szinkronban.!
                    if (service.Status == ServiceControllerStatus.Stopped) { StartThis(service); }
                    else
                    {
                        _myOwnerPlugin.LogThis($"Service status of {service.ServiceName} is {service.Status}, start is skipped.", null, null, LogLevel.Verbose, this.GetType());
                    _myOwnerPlugin.LogThis($"Cycle finished for service {service.ServiceName}. Check cycle lenght {DateTime.UtcNow.Subtract(start).ToString()}", null, null, LogLevel.Verbose, this.GetType());
                    }
                }
                catch (Exception ex)
                {
                    _myOwnerPlugin.LogThis($"Exception occured in service status check cycle for service {Properties.ServiceName}!!!", null, ex, LogLevel.Error, this.GetType());
                }
            }
            finally {MonitorTimerStart();}
        }

        /// <summary>
        /// Újraindítja és logolja az ellenőrzési ciklus indítás időzítőt
        /// </summary>
        private void MonitorTimerStart()
        {
            double _modifyer = (double)(120 - DateTime.Now.Ticks % 40) / 100;
            _timer.Interval = Properties.CheckInterval.TotalMilliseconds * _modifyer;
            _timer.Start();
            _myOwnerPlugin.LogThis($"Check interval timer restarted for service: {Properties.ServiceName}, interval: {_timer.Interval}ms", null, null, LogLevel.Debug, this.GetType());
        }

        /// <summary>
        /// Eloindítja a service-t, és megvárja míg elindul
        /// </summary>
        /// <param name="service">Az elindítandó service</param>
        private void StartThis(ServiceController service)
        {
            CreateServiceStartSemafor(service);
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
            CreateServiceStartSemafor(service);
            _myOwnerPlugin.LogThis($"Restart of service {service.ServiceName} was succesfull!", null, null, LogLevel.Warning, this.GetType());
        }

        /// <summary>
        /// Beteszi a Redisbe a service indítás semaforját
        /// </summary>
        /// <param name="service"></param>
        private void CreateServiceStartSemafor(ServiceController service)
        {
            Dictionary<string, string> logData = new Dictionary<string, string>();
            TimeSpan redisSemaforTime = _myOwnerPlugin.Configuration.DefaultRedisSemaforTime;
            string servicenameforSemafor = service.ServiceName;
            var serviceDefinition = _myOwnerPlugin.Configuration.GetControlledService(service.ServiceName);
            if (serviceDefinition == null)
            {
                serviceDefinition = _myOwnerPlugin.Configuration.GetControlledService(service.DisplayName);
            }
            if (serviceDefinition != null)
            {
                redisSemaforTime = serviceDefinition.CreateRedisSemaforTime;
                servicenameforSemafor = serviceDefinition.ServiceName;
            }
            string semaforName = BuildSemaforName(servicenameforSemafor);
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
                    string semaforvalue = $"from:{DateTime.Now}, len={Properties.CreateRedisSemaforTime}"; ;
                    redisDb.StringSet(semaforName, semaforvalue, Properties.CreateRedisSemaforTime);
                    _myOwnerPlugin.LogThis($"Redis semafor is created.", logData, null, LogLevel.Information, this.GetType());
                }
            }
            if (Properties.DependenciesSemafor && service.ServicesDependedOn.Length > 0)
            {
                foreach (var dService in service.ServicesDependedOn)
                {
                    CreateServiceStartSemafor(dService);
                }
            }
        }

        /// <summary>
        /// Redisen használt semafor kulcs
        /// </summary>
        private string BuildSemaforName(string serviceName)
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
                _myOwnerPlugin.LogThis($"Disposing listener for service: {Properties.ServiceName}", null, null, LogLevel.Debug, this.GetType());
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
