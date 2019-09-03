using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Diagnostics;
using System.ServiceProcess;
using System.IO;
using StackExchange.Redis;

using Vrh.LinqXMLProcessor.Base;
using Vrh.ApplicationContainer;
using Vrh.Logger;

using IVServiceWatchdog.InterventionService;

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
        private IVServiceWatchdogPlugin() {    EndLoad();    }

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
            if (MyStatus == PluginStateEnum.Starting || MyStatus == PluginStateEnum.Running)  {    return;    }
            BeginStart();
            try
            {
                // Implement Start logic here 
                string configParameterFile = _myData.InstanceConfig;
                if (string.IsNullOrEmpty(configParameterFile)) {   configParameterFile = _myData.Type.PluginConfig;   }
                _errorCount = 0;
                _configuration = new IVServiceWatchdogParameterFileProcessor(configParameterFile);
                _configuration.ConfigProcessorEvent += ConfigProcessorEvent;
                _lazyRedisConnection = new Lazy<ConnectionMultiplexer>(RedisConnectionInitializer);

                ThreadStart ts = new ThreadStart(WatchdogProcess);
                _process = new Thread(ts) {   Name = string.Format("Thread ({0})", _myData.Id)   };
                _process.Start();
                if (_configuration.CheckInterval > 0)
                {
                    int timervalue = _configuration.CheckInterval;
                    if (_configuration.StartDelayTime > TimeSpan.Zero)
                    {
                        timervalue += Convert.ToInt32(_configuration.StartDelayTime.TotalMilliseconds);
                        LogThis($"Check timer first check delayed with {_configuration.StartDelayTime}", null, null, LogLevel.Information, this.GetType());
                    }
                    _timer = new System.Timers.Timer(timervalue);
                    _timer.Elapsed += CheckTimeElapsed;
                    _timer.Start();
                    LogThis($"Check timer first start with timeout {_timer.Interval}", null, null, LogLevel.Information, this.GetType());
                }
                base.Start();
            }
            catch (Exception ex) {    SetErrorState(ex);    }
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
            var logData = new Dictionary<string, string>();
            logData.Add("Service name", _configuration.WindowsServiceName);
            LogThis($"Started: {Thread.CurrentThread.Name}", logData, null, LogLevel.Debug, this.GetType());
            _stopped = false;
            while (!disposedValue && !_stopped)
            {
                try
                {
                    if (WaitHandle.WaitAny(new WaitHandle[2] { _itsworktimeAutoResetEvent, _stoppedAutoResetEvent }) == 0)
                    {
                        try
                        {
                            HealthCheck();
                        }
                        finally
                        {
                            _timer.Interval = _configuration.CheckInterval;
                            _timer.Start();
                            LogThis($"Check timer restart with timeout {_timer.Interval}", null, null, LogLevel.Information, this.GetType());
                        }
                    }
                }
                catch(Exception ex)
                {
                    LogThis($"Exception occured!", null, ex, LogLevel.Error, this.GetType());
                }
            }
            logData.Add("disposed", disposedValue.ToString());
            logData.Add("stopped", _stopped.ToString());
            LogThis($"Exited: {Thread.CurrentThread.Name}", logData, null, LogLevel.Debug);
        }

        private string BuildSemaforName(string serviceName)
        {
            return $"Service.Starter.Semafor.{serviceName}";
        }
        
        /// <summary>
        /// Megmondja, van-e Redisben semafor erre a checkre
        /// </summary>
        /// <returns></returns>
        private bool HasRedisSemafor()
        {
            Dictionary<string, string> logData = new Dictionary<string, string>();
            logData.Add("Service name", _configuration.WindowsServiceName);
            try
            {
                var redisDb = RedisConnection?.GetDatabase();
                if (redisDb != null)
                {
                    string semaforname = BuildSemaforName(_configuration.WindowsServiceName);
                    var semaforvalue = redisDb.StringGet(semaforname);
                    if (!semaforvalue.IsNullOrEmpty)
                    {
                        LogThis($"Redis semafor '{semaforname}' exists. Semafor timestamp '{semaforvalue}'. Health check due, but skipped.", logData, null, LogLevel.Information, this.GetType());
                        return true;
                    }
                }
            }
            catch(Exception ex) {    LogThis($"Exception during checking semafor.", logData, ex, LogLevel.Error, this.GetType());    }
            return false;
        }

        /// <summary>
        /// Ellenőrzi, hogy a definiált service telepítve van-e és a fut-e a hozzá tartozó process
        /// </summary>
        /// <returns></returns>
        private int GetHostProcessPid()
        {
            Dictionary<string, string> logData = new Dictionary<string, string>();
            logData.Add("Service name", _configuration.WindowsServiceName);
            ServiceController[] services = ServiceController.GetServices();
            ServiceController service = services.FirstOrDefault();
            uint? processId = null;
            string qry = "SELECT PROCESSID FROM WIN32_SERVICE WHERE NAME = '" + _configuration.WindowsServiceName + "'";
            System.Management.ManagementObjectSearcher searcher = new System.Management.ManagementObjectSearcher(qry);
            foreach (System.Management.ManagementObject mngntObj in searcher.Get()) {    processId = (uint)mngntObj["PROCESSID"];    }

            if (!processId.HasValue)
            {
                LogThis($"Service is not installed in this machine!", logData, null, LogLevel.Warning, this.GetType());
                return -1;
            }

            if (processId == 0)
            {
                LogThis($"Process not found for service!", logData, null, LogLevel.Warning, this.GetType());
                return -1;
            }
            return (int)processId.Value;
        }

        /// <summary>
        /// Ellenőrzi az Intervention szolgáltatás müködőképességét
        /// </summary>
        private void HealthCheck()
        {
            if (!HasRedisSemafor())
            {
                if (!_configuration.CheckThreadNumberEnabled && !_configuration.CheckMemoryUsageEnabled && !_configuration.CheckCPUusageEnabled && !_configuration.CheckWCFResponseTimeEnabled) { return; }

                Dictionary<string, string> logData = new Dictionary<string, string>();
                logData.Add("Service name", _configuration.WindowsServiceName);
                Dictionary<string, string> logData2 = new Dictionary<string, string>();
                LogThis($"Process health check started", logData, null, LogLevel.Verbose, this.GetType());

                bool _anytestError = false;
                _anytestError = _anytestError || HealthCheck_ThreadMaximumExceeded();
                _anytestError = _anytestError || HealthCheck_MemoryMaximumExceeded();
                _anytestError = _anytestError || HealthCheck_CPUusageMaximumExceeded();
                _anytestError = _anytestError || HealthCheck_WCFResponseTimeMaximumExceeded();

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
                            LogThis($"Failed combined health checks lapsed, counter restarted.", logData2, null, LogLevel.Verbose, this.GetType());
                            _errorCount = 1;
                        }
                    }
                    _errorCount++;
                    _lastErrorTimeStamp = DateTime.UtcNow;
                    logData.Add("Current number of failed checks", $"{_errorCount}");
                    logData.Add("Maximum number of failed checks", $"{_configuration.MaxErrorCount}");
                    LogThis($"Combined health check result: ERROR!", logData, null, LogLevel.Error, this.GetType());
                }
                else { LogThis($"Combined health check result: OK!", logData, null, LogLevel.Verbose, this.GetType()); }

                if (_errorCount >= _configuration.MaxErrorCount)
                {
                    ErrorHandling();
                }
            }
        }
        /// <summary>
        /// Megállapítja, hogy a WCF válaszidő túl hosszú-e
        /// </summary>
        /// <returns></returns>
        private bool HealthCheck_WCFResponseTimeMaximumExceeded()
        {
            if (!_configuration.CheckWCFResponseTimeEnabled) { return false; }
            if (_configuration.MaxWCFResponseTime.TotalMilliseconds <= 0) { return false; }
            Dictionary<string, string> logData = new Dictionary<string, string>();
            logData.Add("Service name", _configuration.WindowsServiceName);
            logData.Add("Maximum response Time", _configuration.MaxWCFResponseTime.ToString());
            try
            {
                InterventionServiceClient interventionService = new InterventionServiceClient();
                DateTime requestTime = DateTime.UtcNow;
                var response = interventionService.GetInterventionedObject(null);
                TimeSpan responseTime = DateTime.UtcNow.Subtract(requestTime);
                logData.Add("Actual response Time", responseTime.ToString());
                if (_configuration.MaxWCFResponseTime < responseTime)
                {
                    string exceptiontext = $"ERROR occured! Actual response time: {responseTime}. Allowed maximum response time: {_configuration.MaxWCFResponseTime}";
                    LogThis(exceptiontext, logData, null, LogLevel.Error, this.GetType());
                    return true;
                }
                else
                {
                    LogThis($"HealthCheck_WCFResponseTimeMaximumExceeded is OK!", logData, null, LogLevel.Verbose, this.GetType());
                    return false;
                }
            }
            catch (System.ServiceModel.EndpointNotFoundException ex)
            {
                LogThis($"Nem található az 'InterventionService' WCF végpont!", logData, null, LogLevel.Error, this.GetType());
                return true;
            }
            catch (Exception ex)
            {
                LogThis($"Exception in execution of HealthCheck_WCFResponseTimeMaximumExceeded!", logData, ex, LogLevel.Error, this.GetType());
                return true;
            }
        }
        /// <summary>
        /// Megállapítja, hogy túl sok szál van-e a processzben
        /// </summary>
        /// <returns></returns>
        private bool HealthCheck_ThreadMaximumExceeded()
        {
            if (!_configuration.CheckThreadNumberEnabled) { return false; }
            if(_configuration.MaximumAllowedThreadNumber == 0) { return false; }
            Dictionary<string, string> logData = new Dictionary<string, string>();
            logData.Add("Service name", _configuration.WindowsServiceName);
            try
            {
                int pid = GetHostProcessPid();
                if (pid == -1) { return false; }
                var process = Process.GetProcessById(pid);
                int threadNumber = process.Threads.Count;
                logData.Add("Process name", process.ProcessName);
                logData.Add("Current thread number", $"{threadNumber}");
                logData.Add("Maximum allowed thread number", $"{_configuration.MaximumAllowedThreadNumber}");
                if (threadNumber > _configuration.MaximumAllowedThreadNumber)
                {
                    string exceptiontext = $"ERROR occured! Current number of threads: {threadNumber}. Allowed maximum number of threads: {_configuration.MaximumAllowedThreadNumber}";
                    LogThis(exceptiontext, logData, null, LogLevel.Error, this.GetType());
                    return true;
                }
                else
                {
                    LogThis($"HealthCheck_ThreadMaximumExceeded is OK.", logData, null, LogLevel.Verbose, this.GetType());
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogThis($"Exception in execution of HealthCheck_ThreadMaximumExceeded!", logData, ex, LogLevel.Error, this.GetType());
                return true;
            }
        }
        /// <summary>
        /// Megállapítja, hogy túl sok memóriát használ-e a processz
        /// </summary>
        /// <returns></returns>
        private bool HealthCheck_MemoryMaximumExceeded()
        {
            if (!_configuration.CheckMemoryUsageEnabled) { return false; }
            if (_configuration.MaximumAllowedMemory == 0) { return false; }
            Dictionary<string, string> logData = new Dictionary<string, string>();
            logData.Add("Service name", _configuration.WindowsServiceName);
            try
            {
                int pid = GetHostProcessPid();
                if (pid == -1) { return false; }
                var process = Process.GetProcessById(pid);

                long memory1 = 0; long memory2 = 0; long memory3 = 0;
                long summemory1 = 0; long summemory2 = 0; long summemory3 = 0; long maxmemory1 = 0;
                PerformanceCounter pc = ProcessPerformanceCounter.GetPerfCounterForProcessId(pid, "Process", "Working Set - Private");
                for (int i = 1; i <= _configuration.MemoryUsageSamples; i++)
                {
                    if (i > 1) { Thread.Sleep(1000); process.Refresh(); }
                    memory1 = (long)pc.NextValue();
                    memory2 = process.PrivateMemorySize64;
                    memory3 = process.WorkingSet64;
                    summemory1 += memory1;
                    summemory2 += memory2;
                    summemory3 += memory3;
                    if (memory1 > maxmemory1) { maxmemory1 = memory1; }
                }
                memory1 = summemory1 / _configuration.MemoryUsageSamples;
                memory2 = summemory2 / _configuration.MemoryUsageSamples;
                memory3 = summemory3 / _configuration.MemoryUsageSamples;

                logData.Add("Process name/PID", $"{process.ProcessName}/{pid}");
                logData.Add("Number of samples", $"{_configuration.MemoryUsageSamples}");
                logData.Add("Average memory usage", $"WorkingSet-Private:{memory1},PrivateMemorySize64:{memory2},WorkingSet64:{memory3}");
                logData.Add("Max memory usage", $"WorkingSet-Private:{maxmemory1}");
                logData.Add("Maximum allowed memory usage", $"{_configuration.MaximumAllowedMemory}");
                if (memory1 > _configuration.MaximumAllowedMemory)
                {
                    string exceptiontext = $"ERROR occured! Current memory usage WorkingSet-Private:{memory1}. Allowed maximum number of memory: {_configuration.MaximumAllowedMemory}";
                    LogThis(exceptiontext, logData, null, LogLevel.Error, this.GetType());
                    return true;
                }
                else
                {
                    LogThis($"HealthCheck_MemoryMaximumExceeded is OK.", logData, null, LogLevel.Verbose, this.GetType());
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogThis($"Exception in execution of HealthCheck_MemoryMaximumExceeded!", logData, ex, LogLevel.Error, this.GetType());
                return true;
            }
        }
        /// <summary>
        /// Megállapítja, hogy túl sok memóriát használ-e a processz
        /// </summary>
        /// <returns></returns>
        private bool HealthCheck_CPUusageMaximumExceeded()
        {
            if (!_configuration.CheckCPUusageEnabled) { return false; }
            if (_configuration.MaximumAllowedCPUusage == 0) { return false; }
            Dictionary<string, string> logData = new Dictionary<string, string>();
            logData.Add("Service name", _configuration.WindowsServiceName);
            try
            {
                int pid = GetHostProcessPid();
                if (pid == -1) { return false; }
                var process = Process.GetProcessById(pid);

                float cpuusagefloat = 0;
                float sumusagefloat = 0;
                float maxcpuusagefloat = 0;
                PerformanceCounter pc = ProcessPerformanceCounter.GetPerfCounterForProcessId(pid, "Process", "% Processor Time");
                for (int i = 1; i <= _configuration.CPUusageSamples; i++)
                {
                    if (i > 1) { Thread.Sleep(50); process.Refresh(); }
                    cpuusagefloat = (long)pc.NextValue() / Environment.ProcessorCount;
                    sumusagefloat += cpuusagefloat;
                    if (cpuusagefloat > maxcpuusagefloat) { maxcpuusagefloat = cpuusagefloat; }
                }
                cpuusagefloat = sumusagefloat / _configuration.CPUusageSamples;
                long cpuusage = (long)cpuusagefloat;

                logData.Add("Process name/PID", $"{process.ProcessName}/{pid}");
                logData.Add("Logical processors", $"{Environment.ProcessorCount}");
                logData.Add("Number of samples", $"{_configuration.CPUusageSamples}");
                logData.Add("Current CPU usage", $"{cpuusagefloat}");
                logData.Add("Maximum CPU usage", $"{maxcpuusagefloat}");
                logData.Add("Maximum allowed CPU usage", $"{_configuration.MaximumAllowedCPUusage}");
                if (cpuusage > _configuration.MaximumAllowedCPUusage)
                {
                    string exceptiontext = $"ERROR occured! Current CPU usage:{cpuusagefloat}. Allowed maximum number of memory: {_configuration.MaximumAllowedCPUusage}";
                    LogThis(exceptiontext, logData, null, LogLevel.Error, this.GetType());
                    return true;
                }
                else
                {
                    LogThis($"HealthCheck_CPUusageMaximumExceeded is OK.", logData, null, LogLevel.Verbose, this.GetType());
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogThis($"Exception in execution of HealthCheck_CPUusageMaximumExceeded!", logData, ex, LogLevel.Error, this.GetType());
                return true;
            }
        }

        /// <summary>
        /// Hibakezelés 
        /// </summary>
        private void ErrorHandling()
        {
            int pid = GetHostProcessPid(); if (pid == -1) { return; }
            var process = Process.GetProcessById(pid);
            Dictionary<string, string> logData = new Dictionary<string, string>();
            logData.Add("Service name", _configuration.WindowsServiceName);
            logData.Add("Process name/PID", $"{process.ProcessName}/{pid}");
            Dictionary<string, string> logData2 = new Dictionary<string, string>();
            try
            {
                _errorCount = 0;
                logData2.Clear(); logData.ToList().ForEach(x => logData2.Add(x.Key, x.Value));
                logData2.Add("CreateDumpRequested", $"{_configuration.CreateDump}");
                logData2.Add("KillProcessRequested", $"{_configuration.KillProcess}");
                LogThis($"Start error handling...", logData2, null, LogLevel.Warning, this.GetType());

                if (_configuration.CreateDump)
                {
                    LogThis($"Create dump file from process.", logData, null, LogLevel.Verbose, this.GetType());
                    Process p = new Process();
                    p.StartInfo.FileName = Path.Combine(_configuration.ProcdumpPath, "procdump.exe");
                    p.StartInfo.Arguments = $"-ma -o {pid} {_configuration.DumpTargetPath}";
                    //p.StartInfo.FileName = "";
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.Start();
                    string output = p.StandardOutput.ReadToEnd();
                    while (!p.HasExited)
                    {
                        LogThis($" Waiting for procdump.exe to complete.", logData, null, LogLevel.Verbose, this.GetType());
                        p.WaitForExit();
                    }
                    int i = 0;
                    logData2.Clear();logData.ToList().ForEach(x => logData2.Add(x.Key, x.Value));
                    foreach (var line in output.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        logData2.Add($"Line{i}", line);
                        i++;
                    }
                    LogThis($"Dump create DONE.", logData2, null, LogLevel.Information, this.GetType());
                }
                if (_configuration.KillProcess)
                {
                    while (!process.HasExited)
                    {
                        LogThis($"Kill process.", logData, null, LogLevel.Verbose, this.GetType());
                        process.Kill();
                        process.WaitForExit();
                    }
                }
            }
            catch (Exception ex)
            {
                LogThis($"Exception occured in ErrorHandling.", logData, ex, LogLevel.Error, this.GetType());
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
            if (!string.IsNullOrEmpty(_configuration.RedisConnection))
            {
                for (int i=1;i<_configuration.RedisconnectRetries;i++)
                {
                    try
                    {
                        var cm = ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(_configuration.RedisConnection, true));
                        cm.PreserveAsyncOrder = false;
                        return cm;
                    }
                    catch (Exception ex) {   LogThis($"Attempting ({i} of {_configuration.RedisconnectRetries}) to establish connection to Redis server {_configuration.RedisConnection}", null, ex, LogLevel.Warning, this.GetType());   }
                }
                LogThis($"Error to connect to Redis server {_configuration.RedisConnection}. All {_configuration.RedisconnectRetries} connect attempt failed.", null, null, LogLevel.Fatal, this.GetType());
                return null;
            }
            else
            {
                LogThis($"No Redis connection string is specified. No connection to Redis server {_configuration.RedisConnection} is established.", null, null, LogLevel.Fatal, this.GetType());
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
    public class ProcessPerformanceCounter
    {
        public static PerformanceCounter GetPerfCounterForProcessId(int processId, string processcounterCategory, string processCounterName)
        {
            string instance = GetInstanceNameForProcessId(processId);
            if (string.IsNullOrEmpty(instance)) {   return null;   }

            return new PerformanceCounter(processcounterCategory, processCounterName, instance);
        }

        public static string GetInstanceNameForProcessId(int processId)
        {
            string processName = Path.GetFileNameWithoutExtension(Process.GetProcessById(processId).ProcessName);

            PerformanceCounterCategory cat = new PerformanceCounterCategory("Process");
            string[] instances = cat.GetInstanceNames().Where(inst => inst.StartsWith(processName)).ToArray();

            foreach (string instance in instances)
            {
                if ((int)(new PerformanceCounter("Process", "ID Process", instance, true)).RawValue == processId) { return instance; }

                //using (PerformanceCounter cnt = new PerformanceCounter("Process", "ID Process", instance, true))
                //{
                //    if ((int)cnt.RawValue == processId) { return instance; }
                //}
            }
            return null;
        }
    }
}
