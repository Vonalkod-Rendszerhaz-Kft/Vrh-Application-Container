using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Vrh.ApplicationContainer;
using Vrh.Logger;
using System.Timers;
using Vrh.LinqXMLProcessor.Base;
using IVServiceWatchdog.InterventionService;
using System.Diagnostics;
using System.ServiceProcess;
using System.IO;
using StackExchange.Redis;

namespace IVServiceWatchdog
{
    /// <summary>
    /// IVConnector Service watchdog ApplicationContainer plugin for Lear ALM system
    /// </summary>
    class IVServiceWatchdogPlugin : PluginAncestor
    {
        /// <summary>
        /// Constructor
        /// </summary>
        private IVServiceWatchdogPlugin()
        {
            EndLoad();
        }

        /// <summary>
        /// Factory
        /// </summary>
        /// <param name="instanceDefinition">A példány definiciója</param>
        /// <param name="instanceData">Not used in this plugin</param>
        /// <returns></returns>
        public static IVServiceWatchdogPlugin IVServiceWatchdogPluginFactory(InstanceDefinition instanceDefinition, Object instanceData)
        {
            var instance = new IVServiceWatchdogPlugin();
            instance._myData = instanceDefinition;
            return instance;
        }

        /// <summary>
        /// IPlugin.Start
        /// </summary>
        public override void Start()
        {
            if (MyStatus == PluginStateEnum.Starting || MyStatus == PluginStateEnum.Running)
            {
                return;
            }
            BeginStart();
            try
            {
                // Implement Start logic here 
                string configParameterFile = _myData.InstanceConfig;
                if (String.IsNullOrEmpty(configParameterFile))
                {
                    configParameterFile = _myData.Type.PluginConfig;
                }
                _errorCount = 0;
                _configuration = new IVServiceWatchdogParameterFileProcessor(configParameterFile);
                _configuration.ConfigProcessorEvent += ConfigProcessorEvent;
                _lazyRedisConnection = new Lazy<ConnectionMultiplexer>(RedisConnectionInitializer);

                ThreadStart ts = new ThreadStart(WatchdogProcess);
                _process = new Thread(ts);
                _process.Name = String.Format("IVService Watchdog Process Thread ({0})", _myData.Id);
                _process.Start();
                if (_configuration.CheckInterval > 0)
                {
                    _timer = new System.Timers.Timer(_configuration.CheckInterval);
                    _timer.Elapsed += CheckTimeElapsed;
                    _timer.Start();
                }
                base.Start();
            }
            catch (Exception ex)
            {
                SetErrorState(ex);
            }
        }

        /// <summary>
        /// IPlugin.Stop
        /// </summary>
        public override void Stop()
        {
            if (MyStatus == PluginStateEnum.Stopping || MyStatus == PluginStateEnum.Loaded)
            {
                return;
            }
            BeginStop();
            try
            {
                // Implement stop logic here                
                if (_timer != null)
                {
                    _timer.Stop();
                    _timer.Elapsed -= CheckTimeElapsed;
                    _timer.Dispose();
                    _timer = null;
                }
                _stopped = true;
                _stoppedAutoResetEvent.Set();
                _configuration.ConfigProcessorEvent -= ConfigProcessorEvent;
                _configuration.Dispose();
                if (RedisConnection != null)
                {
                    lock (RedisConnection)
                    {
                        _lazyRedisConnection.Value.Close(true);
                        _lazyRedisConnection.Value.Dispose();
                        _lazyRedisConnection = null;
                        _lazyRedisConnection = new Lazy<ConnectionMultiplexer>(() => { return null; });
                    }
                }
                base.Stop();
            }
            catch (Exception ex)
            {
                SetErrorState(ex);
            }
        }

        /// <summary>
        /// Redis connection
        /// </summary>
        internal ConnectionMultiplexer RedisConnection
        {
            get
            {
                return _lazyRedisConnection.Value;
            }
        }

        /// <summary>
        /// A konfiguráció feldolgozás üzeneteit logoló metódus
        /// </summary>
        /// <param name="e">Eseményargumentum</param>
        private void ConfigProcessorEvent(Vrh.LinqXMLProcessor.Base.ConfigProcessorEventArgs e)
        {
            LogLevel level =
                e.Exception.GetType().Name == typeof(ConfigProcessorWarning).Name
                    ? LogLevel.Warning
                    : LogLevel.Error;
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "ConfigProcessor class", e.ConfigProcessor },
                { "Config file", e.ConfigFile },
            };
            LogThis($"Configuration issue: {e.Message}", data, e.Exception, level);
        }

        /// <summary>
        /// Jelzi, hogy letelt a beállított időzítés és ellenőrizni kell a szolgáltatás elérhetőségét
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckTimeElapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();
            _itsworktimeAutoResetEvent.Set();
        }

        /// <summary>
        /// A plugin workerszálla (ezt  ametósust futatja, running állapotban)
        /// </summary>
        private void WatchdogProcess()
        {
            LogThis($"IVService watchdog process started: {Thread.CurrentThread.Name}", null, null, LogLevel.Debug, this.GetType());
            _stopped = false;
            while (!disposedValue && !_stopped)
            {
                try
                {
                    if (WaitHandle.WaitAny(new WaitHandle[2] { _itsworktimeAutoResetEvent, _stoppedAutoResetEvent }) == 0)
                    {
                        try
                        {
                            Check();
                        }
                        finally
                        {
                            _timer.Start();
                        }
                    }
                }
                catch(Exception ex)
                {
                    LogThis("Exception occured!", null, ex, LogLevel.Error, this.GetType());
                }
            }
            var logData = new Dictionary<string, string>();
            logData.Add("disposed", disposedValue.ToString());
            logData.Add("stopped", _stopped.ToString());
            LogThis($"IVService watchdog process exited: {Thread.CurrentThread.Name}", logData, null, LogLevel.Debug);
        }

        /// <summary>
        /// Ellenőrzi az Intervention szolgáltatás müködőképességét
        /// </summary>
        private void Check()
        {
            if (!HasRedisSemafor())
            {
                LogThis("Check started", null, null, LogLevel.Verbose, this.GetType());
                Exception occuredException = null;
                DateTime checkStart = DateTime.UtcNow;                
                var logData = new Dictionary<string, string>();
                if (_configuration.MinimalResponseTime.TotalMilliseconds > 0)
                {
                    try
                    {
                        InterventionServiceClient interventionService = new InterventionServiceClient();
                        var response = interventionService.GetInterventionedObject(null);
                        TimeSpan responseTime = DateTime.UtcNow.Subtract(checkStart);
                        if (_configuration.MinimalResponseTime < responseTime)
                        {
                            occuredException = new Exception($"Response time is too long: {responseTime} (Allowed maximum response time is: {_configuration.MinimalResponseTime})");
                        }
                        logData.Add("Response Time", responseTime.ToString());
                    }
                    catch (Exception ex)
                    {
                        occuredException = ex;
                    }
                }
                int pid = GetHostProcessPid();
                if (TooManyThreadNumber(pid))
                {
                    occuredException = new Exception("There is too many thread!");
                }
                logData.Add("Count of error", $"{_errorCount}");
                logData.Add("Maximum error count", $"{_configuration.MaxErrorCount}");
                if (occuredException == null)
                {                    
                    LogThis("Check OK!", logData, null, LogLevel.Information, this.GetType());
                }
                else
                {
                    if (_lastErrorTimeStamp != null &&
                        _configuration.LapsesInterval.TotalMilliseconds > 0 &&
                        DateTime.UtcNow.Subtract(_lastErrorTimeStamp.Value) > _configuration.LapsesInterval)
                    {
                        _errorCount = 0;
                    }
                    _lastErrorTimeStamp = DateTime.UtcNow;
                    _errorCount++;
                    LogThis("Check NOK!", logData, occuredException, LogLevel.Error, this.GetType());
                }
                if (_errorCount >= _configuration.MaxErrorCount)
                {                    
                    if (pid > -1)
                    {
                        ErrorHandling(pid);
                    }
                }
            }
        }

        /// <summary>
        /// Megmondja, van-e Redisben semafor erre a checkre
        /// </summary>
        /// <returns></returns>
        private bool HasRedisSemafor()
        {
            try
            {
                var redisDb = RedisConnection?.GetDatabase();
                if (redisDb != null)
                {
                    var key = redisDb.StringGet($"Service.Starter.Semafor.{_configuration.WindowsServiceName}");
                    if (!key.IsNullOrEmpty)
                    {
                        LogThis($"There is Redis semafor, with '{key}' valaue. Check is skiped.", null, null, LogLevel.Information, this.GetType());
                        return true;
                    }
                }
            }
            catch(Exception ex)
            {
                LogThis("Error occured!", null, ex, LogLevel.Error, this.GetType());
            }
            return false;
        }

        /// <summary>
        /// Ellenőrzi, hogy a definiált service telepítve van-e és a fut-e a hozzá tartozó process
        /// </summary>
        /// <returns></returns>
        private int GetHostProcessPid()
        {
            ServiceController[] services = ServiceController.GetServices();
            ServiceController service = services.FirstOrDefault();
            uint? processId = null;
            string qry = "SELECT PROCESSID FROM WIN32_SERVICE WHERE NAME = '" + _configuration.WindowsServiceName + "'";
            System.Management.ManagementObjectSearcher searcher = new System.Management.ManagementObjectSearcher(qry);
            foreach (System.Management.ManagementObject mngntObj in searcher.Get())
            {
                processId = (uint)mngntObj["PROCESSID"];
            }
            if (!processId.HasValue)
            {
                LogThis($"'{_configuration.WindowsServiceName}' windows service is not installed in this machine!", null, null, LogLevel.Warning, this.GetType());
                return -1;
            }
            if (processId == 0)
            {
                LogThis($"Process not found for '{_configuration.WindowsServiceName}' service!", null, null, LogLevel.Warning, this.GetType());
                return -1;
            }
            return (int)processId.Value;
        }

        /// <summary>
        /// Megállapítja, hogy túl sok száll van-e a processzben
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        private bool TooManyThreadNumber(int pid)
        {
            if (pid == -1 || _configuration.MaximumAlloewedThreadNumber == 0)
            {
                return false;
            }
            var process = Process.GetProcessById(pid);
            int threadNumber = process.Threads.Count;
            Dictionary<string, string> logData = new Dictionary<string, string>();
            logData.Add("Current thread number", $"{threadNumber}");
            logData.Add("Maximum allowed thread number", $"{_configuration.MaximumAlloewedThreadNumber}");
            if (threadNumber > _configuration.MaximumAlloewedThreadNumber)
            {
                LogThis("There is too Many thread number!", logData, null, LogLevel.Error, this.GetType());
                return true;
            }
            else
            {
                LogThis("Check thread number, and it's OK.", logData, null, LogLevel.Verbose, this.GetType());
                return false;
            }
        } 

        /// <summary>
        /// Hibakezelés 
        /// </summary>
        private void ErrorHandling(int pid)
        {
            try
            {
                _errorCount = 0;
                Dictionary<string, string> logData = new Dictionary<string, string>();
                logData.Add("CreateDumpNeed", $"{_configuration.CreateDumpNeed}");
                logData.Add("KillProcessNeed", $"{_configuration.KillProcessNeed}");
                LogThis($"There is Error! Start error handling...", logData, null, LogLevel.Warning, this.GetType());
                logData.Clear();
                var process = Process.GetProcessById(pid);                
                if (_configuration.CreateDumpNeed)
                {
                    LogThis($"Create dump file from process: {process.ProcessName}", null, null, LogLevel.Verbose, this.GetType());
                    Process p = new Process();
                    p.StartInfo.FileName = Path.Combine(_configuration.ProcdumpPath, "procdump.exe");
                    p.StartInfo.Arguments = $"-ma -o {pid} {_configuration.DumpTargetPath}";
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.Start();
                    string output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();
                    int i = 0;
                    foreach (var line in output.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        logData.Add($"Line{i}", line);
                        i++;
                    }
                    LogThis("Dump create is success", logData, null, LogLevel.Information, this.GetType());
                    logData.Clear();
                }
                if (_configuration.KillProcessNeed)
                {
                    LogThis($"Kill the  process: {process.ProcessName}", null, null, LogLevel.Verbose, this.GetType());
                    process.Kill();
                }
            }
            catch (Exception ex)
            {
                LogThis("Error occured!!!", null, ex, LogLevel.Error, this.GetType());
            }
        }

        /// <summary>
        /// A Watchdog által használt konfiguráció
        /// </summary>
        private IVServiceWatchdogParameterFileProcessor _configuration;

        /// <summary>
        /// Ennyi hiba lépett fel
        /// </summary>
        private int _errorCount = 0;

        /// <summary>
        /// Utoljára fellépett hiba időbélyege
        /// </summary>
        private DateTime? _lastErrorTimeStamp = null;

        /// <summary>
        /// Folymatot futtató szál
        /// </summary>
        private Thread _process;

        /// <summary>
        /// Timer, amely a beálított időközönként "üt"
        /// </summary>
        private System.Timers.Timer _timer = null;

        /// <summary>
        /// Jelzi, hogy a plugin áll
        /// </summary>
        private bool _stopped = true;

        /// <summary>
        /// Semafor, amit az időzítésnek megfelelő időnként nyílik, majd újra lezáródik
        /// </summary>
        private AutoResetEvent _itsworktimeAutoResetEvent = new AutoResetEvent(false);

        /// <summary>
        /// Semafor, ami akkor nyilík, ha plugint leállították
        /// </summary>
        private AutoResetEvent _stoppedAutoResetEvent = new AutoResetEvent(false);

        /// <summary>
        /// Lazy StackExchange.Redis connection multiplexer
        /// </summary>
        private Lazy<ConnectionMultiplexer> _lazyRedisConnection;

        /// <summary>
        /// ConnectionMultiplexer factory a Lazzy inicializációhoz
        /// </summary>
        /// <returns></returns>
        private ConnectionMultiplexer RedisConnectionInitializer()
        {
            if (!String.IsNullOrEmpty(_configuration.RedisConnection))
            {
                try
                {
                    var cm = ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(_configuration.RedisConnection, true));
                    cm.PreserveAsyncOrder = false;
                    return cm;
                }
                catch (Exception ex)
                {
                    LogThis($"Error in connect to Redis server: {_configuration.RedisConnection}", null, ex, LogLevel.Fatal, this.GetType());
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        #region IDisposable Support
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    try
                    {
                        BeginDispose();
                        // TODO: dispose managed state (managed objects).
                        Stop();
                    }
                    finally
                    {
                        base.Dispose(disposing);
                    }
                }
                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~TestPlugin() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public override void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
