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
                _process.Name = String.Format(GetLogHeading() + " Thread ({0})", _myData.Id);
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
            string Logheading = GetLogHeading();
            LogLevel level =
                e.Exception.GetType().Name == typeof(ConfigProcessorWarning).Name
                    ? LogLevel.Warning
                    : LogLevel.Error;
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "ConfigProcessor class", e.ConfigProcessor },
                { "Config file", e.ConfigFile },
            };
            LogThis($"{Logheading} Configuration issue: {e.Message}", data, e.Exception, level);
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
            string Logheading = GetLogHeading();
            var logData = new Dictionary<string, string>();
            logData.Add("Service name", _configuration.WindowsServiceName);
            LogThis($"{Logheading} Started: {Thread.CurrentThread.Name}", logData, null, LogLevel.Debug, this.GetType());
            _stopped = false;
            while (!disposedValue && !_stopped)
            {
                try
                {
                    if (WaitHandle.WaitAny(new WaitHandle[2] { _itsworktimeAutoResetEvent, _stoppedAutoResetEvent }) == 0)
                    {
                        try
                        {
                            ProcessHealthCheck();
                        }
                        finally
                        {
                            _timer.Start();
                        }
                    }
                }
                catch(Exception ex)
                {
                    LogThis($"{Logheading} Exception occured!", null, ex, LogLevel.Error, this.GetType());
                }
            }
            logData.Add("disposed", disposedValue.ToString());
            logData.Add("stopped", _stopped.ToString());
            LogThis($"{Logheading} Exited: {Thread.CurrentThread.Name}", logData, null, LogLevel.Debug);
        }

        /// <summary>
        /// Ellenőrzi az Intervention szolgáltatás müködőképességét
        /// </summary>
        private void ProcessHealthCheck()
        {
            if (!HasRedisSemafor())
            {
                string Logheading = GetLogHeading();
                Dictionary<string, string> logData = new Dictionary<string, string>();
                logData.Add("Service name", _configuration.WindowsServiceName);
                Dictionary<string, string> logData2 = new Dictionary<string, string>();
                LogThis($"{Logheading} Process health check started", logData, null, LogLevel.Verbose, this.GetType());

                bool _anytestError = false;
                _anytestError = _anytestError || HealthCheck_ResponseTimeMaximumExceeded();
                _anytestError = _anytestError || HealthCheck_ThreadMaximumExceeded();

                if (_anytestError)
                {
                    if (_lastErrorTimeStamp != null)
                    {
                        TimeSpan _lastNoIssuePeriodeLength = DateTime.UtcNow.Subtract(_lastErrorTimeStamp.Value);
                        if (_configuration.LapsesInterval.TotalMilliseconds > 0 && _lastNoIssuePeriodeLength > _configuration.LapsesInterval)
                        {
                            logData2.Clear(); logData.ToList().ForEach(x => logData2.Add(x.Key, x.Value));
                            logData2.Add("No-issue periode length", _lastNoIssuePeriodeLength.ToString());
                            logData2.Add("Lapse timeout", _configuration.LapsesInterval.ToString());
                            LogThis($"{Logheading} Failed combined health checks lapsed, counter restarted.", logData2, null, LogLevel.Verbose, this.GetType());
                            _errorCount = 1;
                        }
                    }
                    _errorCount++;
                    _lastErrorTimeStamp = DateTime.UtcNow;
                    logData.Add("Current number of failed checks", $"{_errorCount}");
                    logData.Add("Maximum number of failed checks", $"{_configuration.MaxErrorCount}");
                    LogThis($"{Logheading} Combined health check result: ERROR!", logData, null, LogLevel.Error, this.GetType());
                }
                else { LogThis($"{Logheading} Combined health check result: OK!", logData, null, LogLevel.Verbose, this.GetType()); }

                if (_errorCount >= _configuration.MaxErrorCount)
                {
                    ErrorHandling();
                }
            }
        }

        /// <summary>
        /// Megmondja, van-e Redisben semafor erre a checkre
        /// </summary>
        /// <returns></returns>
        private bool HasRedisSemafor()
        {
            string Logheading = GetLogHeading();
            Dictionary<string, string> logData = new Dictionary<string, string>();
            logData.Add("Service name", _configuration.WindowsServiceName);
            try
            {
                var redisDb = RedisConnection?.GetDatabase();
                if (redisDb != null)
                {
                    var key = redisDb.StringGet($"Service.Starter.Semafor.{_configuration.WindowsServiceName}");
                    if (!key.IsNullOrEmpty)
                    {
                        LogThis($"{Logheading} Redis semafor exists. Semafor key:{key}. Health check due, but skipped.", logData, null, LogLevel.Information, this.GetType());
                        return true;
                    }
                }
            }
            catch(Exception ex)
            {
                LogThis($"{Logheading} Exception during checking semafor.", logData, ex, LogLevel.Error, this.GetType());
            }
            return false;
        }

        /// <summary>
        /// Ellenőrzi, hogy a definiált service telepítve van-e és a fut-e a hozzá tartozó process
        /// </summary>
        /// <returns></returns>
        private int GetHostProcessPid()
        {
            string Logheading = GetLogHeading();
            Dictionary<string, string> logData = new Dictionary<string, string>();
            logData.Add("Service name", _configuration.WindowsServiceName);
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
                LogThis($"{Logheading} Service is not installed in this machine!", logData, null, LogLevel.Warning, this.GetType());
                return -1;
            }
            if (processId == 0)
            {
                LogThis($"{Logheading} Process not found for service!", logData, null, LogLevel.Warning, this.GetType());
                return -1;
            }
            return (int)processId.Value;
        }

        /// <summary>
        /// Megállapítja, hogy a WCF válaszidő túl hosszú-e
        /// </summary>
        /// <returns></returns>
        private bool HealthCheck_ResponseTimeMaximumExceeded()
        {
            if (_configuration.MinimalResponseTime.TotalMilliseconds <= 0) { return false; }
            string Logheading = GetLogHeading();
            Dictionary<string, string> logData = new Dictionary<string, string>();
            logData.Add("Service name", _configuration.WindowsServiceName);
            try
            {
                InterventionServiceClient interventionService = new InterventionServiceClient();
                DateTime requestTime = DateTime.UtcNow;
                var response = interventionService.GetInterventionedObject(null);
                TimeSpan responseTime = DateTime.UtcNow.Subtract(requestTime);
                logData.Add("Actual response Time", responseTime.ToString());
                logData.Add("Maximum response Time", _configuration.MinimalResponseTime.ToString());
                if (_configuration.MinimalResponseTime < responseTime)
                {
                    string exceptiontext = $"{Logheading} ERROR occured! Actual response time: {responseTime}. Allowed maximum response time: {_configuration.MinimalResponseTime}";
                    LogThis(exceptiontext, logData, null, LogLevel.Error, this.GetType());
                    return true;
                }
                else
                {
                    LogThis($"{Logheading} is OK!", logData, null, LogLevel.Verbose, this.GetType());
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogThis($"{Logheading} Exception in execution of HealthCheck_ResponseTimeMaximumExceeded!", logData, ex, LogLevel.Error, this.GetType());
                return true;
            }
        }

        /// <summary>
        /// Megállapítja, hogy túl sok száll van-e a processzben
        /// </summary>
        /// <returns></returns>
        private bool HealthCheck_ThreadMaximumExceeded()
        {
            if (_configuration.MaximumAlloewedThreadNumber == 0) { return false; }
            string Logheading = GetLogHeading();
            Dictionary<string, string> logData = new Dictionary<string, string>();
            logData.Add("Service name", _configuration.WindowsServiceName);
            try
            {
                int pid = GetHostProcessPid();
                if (pid == -1 || _configuration.MaximumAlloewedThreadNumber == 0) { return false; }
                var process = Process.GetProcessById(pid);
                int threadNumber = process.Threads.Count;
                logData.Add("Process name", process.ProcessName);
                logData.Add("Current thread number", $"{threadNumber}");
                logData.Add("Maximum allowed thread number", $"{_configuration.MaximumAlloewedThreadNumber}");
                if (threadNumber > _configuration.MaximumAlloewedThreadNumber)
                {
                    string exceptiontext = $"{Logheading} ERROR occured! Current number of threads: {threadNumber}. Allowed maximum number of threads: {_configuration.MaximumAlloewedThreadNumber}";
                    LogThis(exceptiontext, logData, null, LogLevel.Error, this.GetType());
                    return true;
                }
                else
                {
                    LogThis($"{Logheading} is OK.", logData, null, LogLevel.Verbose, this.GetType());
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogThis($"{Logheading} Exception in execution of HealthCheck_ResponseTimeMaximumExceeded!", logData, ex, LogLevel.Error, this.GetType());
                return true;
            }
        }

        /// <summary>
        /// Hibakezelés 
        /// </summary>
        private void ErrorHandling()
        {
            string Logheading = GetLogHeading();
            int pid = GetHostProcessPid(); if (pid == -1) { return; }
            var process = Process.GetProcessById(pid);
            Dictionary<string, string> logData = new Dictionary<string, string>();
            logData.Add("Service name", _configuration.WindowsServiceName);
            logData.Add("Process name", process.ProcessName);
            Dictionary<string, string> logData2 = new Dictionary<string, string>();
            try
            {
                _errorCount = 0;
                logData2.Clear(); logData.ToList().ForEach(x => logData2.Add(x.Key, x.Value));
                logData2.Add("CreateDumpRequested", $"{_configuration.CreateDumpNeed}");
                logData2.Add("KillProcessRequested", $"{_configuration.KillProcessNeed}");
                LogThis($"{Logheading} Start error handling...", logData2, null, LogLevel.Warning, this.GetType());

                if (_configuration.CreateDumpNeed)
                {
                    LogThis($"{Logheading} Create dump file from process.", logData, null, LogLevel.Verbose, this.GetType());
                    Process p = new Process();
                    p.StartInfo.FileName = Path.Combine(_configuration.ProcdumpPath, "procdump.exe");
                    p.StartInfo.Arguments = $"-ma -o {pid} {_configuration.DumpTargetPath}";
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.Start();
                    string output = p.StandardOutput.ReadToEnd();
                    while (!p.HasExited)
                    {
                        LogThis($"{Logheading}  Waiting for procdump.exe to complete.", logData, null, LogLevel.Verbose, this.GetType());
                        p.WaitForExit();
                    }
                    int i = 0;
                    logData2.Clear();logData.ToList().ForEach(x => logData2.Add(x.Key, x.Value));
                    foreach (var line in output.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        logData2.Add($"Line{i}", line);
                        i++;
                    }
                    LogThis($"{Logheading} Dump create DONE.", logData2, null, LogLevel.Information, this.GetType());
                }
                if (_configuration.KillProcessNeed)
                {
                    while (!process.HasExited)
                    {
                        LogThis($"{Logheading} Kill process.", logData, null, LogLevel.Verbose, this.GetType());
                        process.Kill();
                        process.WaitForExit();
                    }
                }
            }
            catch (Exception ex)
            {
                LogThis($"{Logheading} Exception occured in ErrorHandling.", logData, ex, LogLevel.Error, this.GetType());
            }
        }

        /// <summary>
        /// Creates heading for the Watchdog log records.
        /// </summary>
        private string GetLogHeading()
        {
            return $"IVServiceWatchdogPlugin - {(new System.Diagnostics.StackTrace()).GetFrame(1).GetMethod().Name}.";
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
            string Logheading = GetLogHeading();
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
                    LogThis($"{Logheading} Error in connect to Redis server: {_configuration.RedisConnection}", null, ex, LogLevel.Fatal, this.GetType());
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
