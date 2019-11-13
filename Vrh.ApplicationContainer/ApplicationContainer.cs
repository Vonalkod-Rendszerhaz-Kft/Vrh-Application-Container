using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel.Composition.Registration;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.Diagnostics;
using System.Configuration;
using Vrh.Logger;
using System.ServiceModel;
using VRH.Common;
using System.Runtime.CompilerServices;
using System.Threading;
using Newtonsoft.Json;
using Vrh.XmlProcessing;

namespace Vrh.ApplicationContainer
{
    /// <summary>
    /// Aplication container core class
    /// </summary>
    public class ApplicationContainer : IDisposable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="args">indítási argumentumok</param>
        public ApplicationContainer(string[] args)
        {
            _wcfServiceInstance = new ApplicationContainerService();
            _wcfServiceInstance.ApplicationContainerReference = this;
            string inuseby = "TRUE";
            string dummyString = CommandLine.GetCommandLineArgument(args, "-INUSEBY");
            if (dummyString != null) 
            { 
                inuseby = dummyString.ToUpper(); 
            }
            _startupTimeStamp = DateTime.UtcNow;
            string configFile = ConfigurationManager.AppSettings[GetApplicationConfigName(CONFIGURATIONFILE_ELEMENT_NAME)];
            if (string.IsNullOrEmpty(configFile))
            {
                configFile = @"ApplicationContainer.Config.xml";
            }
            _config = new ApplicationContainerConfig(configFile);
            _config.ConfigProcessorEvent += Config_ConfigProcessorEvent;
            
            _errorStack.Capacity = _config.MessageStackSize;
            _infoStack.Capacity = _config.MessageStackSize;
            LoadInstanceFactoryPlugin();
            if (_usedInstanceFactory != null)
            {
                _loadedPluginDefinitions = GetAllPluginDefinition();
                foreach (var pluginType in _loadedPluginDefinitions)
                {
                    foreach (var instance in GetAllDefinedInstances(pluginType))
                    {
                        if (instance.InuseBy.ToUpper() == inuseby || (inuseby == "TRUE" && instance.InuseBy.ToUpper() != "FALSE"))
                        {
                            Task.Run(() => StartUpInstance(instance));
                        }
                    }
                }
            }
            //string wcfbaseaddresslistconnectionstringName = ConfigurationManager.AppSettings[GetApplicationConfigName(WCFBASEADDRESS_ELEMENT_NAME)];
            StartService(_config.WCFHost(_wcfServiceInstance));
            _lastStartupCost = DateTime.UtcNow.Subtract(_startupTimeStamp);
            Dictionary<string, string> data = new Dictionary<string, string>()
                    {
                        { "Used Config file", configFile },
                        { "Full startup time (second)", _lastStartupCost.TotalSeconds.ToString() },
                        { "Location", this.GetType().Assembly.Location },
                        { "Used InstanceFactory", _usedInstanceFactory?.GetType().FullName },
                        { "InstanceFactory version", _usedInstanceFactory?.GetType().Assembly.Version() },
                        { "IntsanceFactory config", _usedInstanceFactory?.Config },
                    };
            LogThis($"Vrh.ApplicationContainer {this.GetType().Assembly.Version()} started.", data, null, LogLevel.Information);
        }

        //private void TestDiagnostic()
        //{
        //    ulong address = 0;
        //    unsafe
        //    {
        //        Object o = (Object)_pluginContainer.FirstOrDefault().Value;
        //        TypedReference tr = __makeref(o);
        //        IntPtr ptr = **(IntPtr**)(&tr);
        //        address = (ulong)ptr;
        //    }
        //    using (DataTarget target = DataTarget.AttachToProcess(
        //        Process.GetCurrentProcess().Id, 5000, AttachFlag.Passive))
        //    {
        //        ClrRuntime runtime = target.ClrVersions.First().CreateRuntime();
        //        //foreach (var item in runtime.AppDomains)
        //        //{
        //        //    Console.WriteLine("Appdomain info:");
        //        //    Console.WriteLine("Name: {0}\nAddress: {1}\n {2},ApplicationBase: {3}\nId: {4}",
        //        //    item.Name, item.Address, item.ApplicationBase, item.ConfigurationFile, item.Id);
        //        //    foreach (var module in item.Modules)
        //        //    {
        //        //        //Console.WriteLine(module.AssemblyName);
        //        //    }
        //        //}
        //        ClrHeap heap = runtime.GetHeap();
        //        foreach (var item in runtime.Threads)
        //        {
        //            Console.WriteLine(item.OSThreadId);
        //        }
        //        var c = Process.GetCurrentProcess();
        //        var t = c.Threads;
        //        foreach (ProcessThread item in t)
        //        {
        //            if (item.TotalProcessorTime == new TimeSpan(0, 0, 0))
        //            {
        //                continue;
        //            }
        //            Console.WriteLine("__________________________________");
        //            ClrThread clrT = runtime.Threads.FirstOrDefault(x => x.OSThreadId == item.Id);
        //            if (clrT != null)
        //            {
        //                Console.WriteLine(clrT.EnumerateStackTrace().FirstOrDefault()?.DisplayString);
        //                Console.WriteLine(clrT.EnumerateStackTrace().LastOrDefault()?.DisplayString);
        //            }
        //            //Console.WriteLine();
        //            Console.WriteLine("ID: {0} StartTime: {1} State: {2} TotalPT: {3} UserPT: {4}, PrivilegedPT: {5} WR: {6}"
        //                , item.Id, item.StartTime, item.ThreadState, item.TotalProcessorTime, item.UserProcessorTime, item.PrivilegedProcessorTime, item.ThreadState == System.Diagnostics.ThreadState.Wait ? item.WaitReason.ToString() : "");
        //        }
        //        Console.WriteLine(heap.TotalHeapSize);
        //        ClrType type = heap.GetObjectType(address);
        //        foreach (var item in type.Fields)
        //        {
        //            Console.WriteLine("{0}: {1}", item.Name, item.GetAddress(address));
        //        }

        //        Console.WriteLine("{0}: {1}, {2}", type.Name, type.GetSize(address), type.IsFinalizable);



        //        //if (!heap.CanWalkHeap)
        //        //{
        //        //    Console.WriteLine("Cannot walk the heap!");
        //        //}
        //        //else
        //        //{
        //        //    foreach (var item in heap.Segments)
        //        //    {
        //        //        Console.WriteLine(item.Length);
        //        //        foreach (ulong obj in item.EnumerateObjectAddresses())
        //        //        {                                
        //        //            ClrType type = heap.GetObjectType(obj);
        //        //            if (type == null)
        //        //                continue;
        //        //            if (type.Name.Contains("System.Collections.Generic.Dictionary<Vrh.ApplicationContainer.InstanceDefinition,Vrh.ApplicationContainer.IPlugin"))
        //        //            {                                    
        //        //                Console.WriteLine("{0}: {1}", type.Name, type.GetSize(obj));
        //        //            }
        //        //        }
        //        //    }
        //        //    //foreach (ClrType item in heap.EnumerateTypes().OrderBy(x => x.Name))
        //        //    //{
        //        //    //    if (item.Name.Contains("Plugin"))
        //        //    //        Console.WriteLine(item.Name);
        //        //    //}
        //        //    //foreach (ulong obj in heap.EnumerateObjectAddresses())
        //        //    //{
        //        //    //    get type = heap.GetObjectType(obj);

        //        //    //    // If heap corruption, continue past this object.
        //        //    //    if (type == null)
        //        //    //        continue;

        //        //    //    ulong size = type.GetSize(obj);
        //        //    //    Console.WriteLine("{0,12:X} {1,8:n0} {2,1:n0} {3}", obj, size, heap.GetObjectGeneration(obj), type.Name);
        //        //    //}
        //        //}
        //    }
        //}

        /// <summary>
        /// Az Application contaner keret állapotleíróját adja vissza
        /// </summary>
        public ApplicationContainerInfo MyInfo
        {
            get
            {
                //TestDiagnostic();
                ApplicationContainerInfo myInfo = new ApplicationContainerInfo()
                {
                    RunningDirectory = Path.GetDirectoryName(this.GetType().Assembly.Location),
                    Description = this.GetType().Assembly.AssemblyAttribute<AssemblyDescriptionAttribute>(),
                    CopyRight = this.GetType().Assembly.AssemblyAttribute<AssemblyCopyrightAttribute>(),
                    Version = this.GetType().Assembly.Version(),
                };
                lock (_instanceLocker)
                {
                    myInfo.InstanceFactoryPlugin = _usedInstanceFactory?.GetType().FullName;
                    myInfo.InstanceFactoryVersion = _usedInstanceFactory?.GetType().Assembly.Version();
                    myInfo.InstanceFactorySettings = _usedInstanceFactory?.Config;
                    myInfo.InstanceFactoryAssembly = _usedInstanceFactory?.GetType().Assembly.Location;
                    myInfo.StartTimeStamp = _startupTimeStamp;
                    myInfo.LastStartupFullTime = _lastStartupCost.TotalSeconds;
                }
                return myInfo;
            }
        }

        /// <summary>
        /// Hibatároló verem
        /// </summary>
        public List<MessageStackEntry> ErrorStack
        {
            get
            {
                lock (_errorStack)
                {
                    return _errorStack.Items;
                }
            }
        }

        /// <summary>
        /// Információtároló verem
        /// </summary>
        public List<MessageStackEntry> InfoStack
        {
            get
            {
                lock (_infoStack)
                {
                    return _infoStack.Items;
                }
            }
        }

        /// <summary>
        /// A definiált és/vagy betöltött pluginok listája
        /// </summary>
        public List<PluginDefinition> DefinedOrLoadedPlugins
        {
            get
            {
                List<PluginDefinition> plugins = new List<PluginDefinition>();
                lock (_instanceLocker)
                {
                    foreach (var plugin in GetAllPluginDefinition())
                    {
                        plugin.LoadedInstanceCount = _pluginContainer.Count(x => x.Key.Type.TypeName == plugin.TypeName && x.Key.Type.Version == plugin.Version);
                        plugin.DefinedInstanceCount = GetAllDefinedInstances(plugin).Count();
                        plugin.Loaded = _loadedPluginDefinitions.Any(x => x.TypeName == plugin.TypeName && x.Version == plugin.Version);
                        plugins.Add(plugin);
                    }
                    // Add not loadable
                    foreach (var noLoaded in _usedInstanceFactory.GetAllPlugin())
                    {
                        if (!plugins.Any(x => x.TypeName == noLoaded.TypeName && x.Version == noLoaded.Version))
                        {
                            noLoaded.LoadedInstanceCount = 0;
                            noLoaded.DefinedInstanceCount = -1;
                            noLoaded.Loaded = false;
                            plugins.Add(noLoaded);
                        }
                    }
                    // Add not defined but loaded
                    foreach (var item in _pluginContainer)
                    {
                        if (!plugins.Any(x => x.TypeName == item.Key.Type.TypeName && x.Version == item.Key.Type.Version))
                        {
                            var droped = item.Key.Type;
                            droped.LoadedInstanceCount = _pluginContainer.Count(x => x.Key.Type.TypeName == item.Key.Type.TypeName && x.Key.Type.Version == item.Key.Type.Version);
                            droped.DefinedInstanceCount = 0;
                            droped.NotDefined = true;
                            plugins.Add(droped);
                        }
                    }
                }
                return plugins;
            }
        }

        /// <summary>
        /// Az instancefactory működéséhez tartozó hibák tárolóverme
        /// </summary>
        public List<MessageStackEntry> InstanceFactoryErrorStack
        {
            get
            {
                if (_usedInstanceFactory == null)
                {
                    return new List<MessageStackEntry>();
                }
                else
                {
                    return _usedInstanceFactory.Errors;
                }
            }
        }

        /// <summary>
        /// Az instancefactory működéséhez tartozó információk tárolóverme
        /// </summary>
        public List<MessageStackEntry> InstanceFactoryInfoStack
        {
            get
            {
                if (_usedInstanceFactory == null)
                {
                    return new List<MessageStackEntry>();
                }
                else
                {
                    return _usedInstanceFactory.Infos;
                }
            }
        }

        /// <summary>
        /// Visszaadja a adott plugin betöltött instance-ait
        /// </summary>
        /// <param name="pluginType">plugin típus</param>
        /// <param name="version">verzió</param>
        /// <returns>a plugin betöltött instanc-ai</returns>
        public List<InstanceDefinition> GetInstances(string pluginType, string version)
        {
            List<InstanceDefinition> instances = new List<InstanceDefinition>();
            foreach (var item in _pluginContainer.Where(x => x.Key.Type.TypeName == pluginType && x.Key.Type.Version == version))
            {
                instances.Add(item.Key);
            }
            return instances;
        }

        /// <summary>
        /// Visszadja az adott plugin instance státusz információit
        /// </summary>
        /// <param name="internalId">plugin instance azonosítója</param>
        /// <returns>státusz információ</returns>
        public PluginStatus GetPluginStatus(Guid internalId)
        {
            return FindPluginInstanceReference(internalId).Status;
        }

        /// <summary>
        /// Visszadja az adott plugin instance működéséhez kapcsolodó hibatároló tartalmát
        /// </summary>
        /// <param name="internalId">plugin instance azonosítója</param>
        /// <returns>habák listája</returns>
        public List<MessageStackEntry> GetPluginInstanceErrors(Guid internalId)
        {
            return FindPluginInstanceReference(internalId).Errors;
        }

        /// <summary>
        /// Visszadja az adott plugin instance működéséhez kapcsolodó információkat
        /// </summary>
        /// <param name="internalId">plugin instance azonosítója</param>
        /// <returns>információk a példány működéséről</returns>
        public List<MessageStackEntry> GetPluginInstanceInfos(Guid internalId)
        {
            return FindPluginInstanceReference(internalId).Infos;
        }

        /// <summary>
        /// Elindítja a plugin működését (IPlugin.Start hívása)
        /// </summary>
        /// <param name="internalId">plugin példény azonosítója</param>
        /// <returns>az indítás sikeres voltát (true) jelzi</returns>
        public bool StartPlugin(Guid internalId)
        {
            var pluginReference = FindPluginInstanceReference(internalId);
            return CallPluginAcctionWithWaitForPluginStateChange(pluginReference, pluginReference.Start);
        }

        /// <summary>
        /// Leállítja az adott plugin működését
        /// </summary>
        /// <param name="internalId">plugin instance azonosítója</param>
        /// <returns>a művelet sikeres (true) / sikertelen (false) voltát jelzi vissza</returns>
        public bool StopPlugin(Guid internalId)
        {
            var pluginReference = FindPluginInstanceReference(internalId);
            return CallPluginAcctionWithWaitForPluginStateChange(pluginReference, pluginReference.Stop);
        }

        /// <summary>
        /// Újratölti a plugin példányt annak tényléges eldobásával, majd újrapéldányosításával
        /// </summary>
        /// <param name="internalId">plugin instance azonosítója</param>
        /// <returns>a művelet sikeres (true) / sikertelen (false) voltát jelzi vissza</returns>
        public bool ReloadPluginInstance(Guid internalId)
        {
            bool retValue = true;
            var plugin = _pluginContainer.FirstOrDefault(x => x.Key.InternalId == internalId);
            if (plugin.Key != null)
            {
                var pluginReference = plugin.Value;
                // Dispose
                retValue = CallPluginAcctionWithWaitForPluginStateChange(pluginReference, pluginReference.Dispose);
                if (!retValue)
                {
                    return false;
                }
            }
            // Load & Start
            StartUpInstance(plugin.Key);
            return true;
        }

        /// <summary>
        /// A 
        /// </summary>
        /// <param name="pluginType"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public bool FollowPluginDefinitionChange(string pluginType, string version)
        {
            var pluginsNow = GetAllPluginDefinition();
            var plugin = pluginsNow.FirstOrDefault(x => x.TypeName == pluginType && x.Version == version);
            if (plugin != null)
            {
                if (plugin.NotDefined)
                {
                    // Drop all undefined
                    List<Guid> pluginInstancesToDrop = new List<Guid>();
                    lock (_instanceLocker)
                    {
                        foreach (var currentPlugin in _pluginContainer.Where(x => x.Key.Type.TypeName == plugin.TypeName
                                                                                    && x.Key.Type.Version == plugin.Version))
                        {
                            pluginInstancesToDrop.Add(currentPlugin.Key.InternalId);
                        }
                    }
                    foreach (var item in pluginInstancesToDrop)
                    {
                        var pluginItem = _pluginContainer.FirstOrDefault(x => x.Key.InternalId == item);
                        while (pluginItem.Key != null)
                        {
                            CallPluginAcctionWithWaitForPluginStateChange(pluginItem.Value, pluginItem.Value.Dispose, throwException: false);
                            lock (_instanceLocker)
                            {
                                pluginItem = _pluginContainer.FirstOrDefault(x => x.Key.InternalId == item);
                            }
                        }
                    }
                    lock (_instanceLocker)
                    {
                        _loadedPluginDefinitions.RemoveAll(x => x.TypeName == pluginType && x.Version == version);
                    }
                }
                else
                {
                    IEnumerable<InstanceDefinition> defineds = GetAllDefinedInstances(plugin);
                    // Create new instances                    
                    foreach (var instance in defineds)
                    {
                        if (!_pluginContainer.Any(x => x.Key.Id == instance.Id
                                                        && x.Key.Type.TypeName == pluginType
                                                        && x.Key.Type.Version == version))
                        {
                            StartUpInstance(instance);
                        }
                    }
                    List<Guid> pluginInstancesToDrop = new List<Guid>();
                    // Drop unwanted
                    lock (_instanceLocker)
                    {                        
                        foreach (var item in _pluginContainer)
                        {
                            if (!defineds.Any(x => x.Id == item.Key.Id
                                                    && x.Type.TypeName == item.Key.Type.TypeName
                                                    && x.Type.Version == item.Key.Type.Version))
                            {
                                pluginInstancesToDrop.Add(item.Key.InternalId);
                            }
                        }
                    }
                    foreach (var item in pluginInstancesToDrop)
                    {
                        var pluginItem = _pluginContainer.FirstOrDefault(x => x.Key.InternalId == item);
                        while (pluginItem.Key != null)
                        {
                            CallPluginAcctionWithWaitForPluginStateChange(pluginItem.Value, pluginItem.Value.Dispose, throwException: false);
                            lock (_instanceLocker)
                            {
                                pluginItem = _pluginContainer.FirstOrDefault(x => x.Key.InternalId == item);
                            }
                        }
                    }
                }
                lock (_instanceLocker)
                {
                    if (!_loadedPluginDefinitions.Any(x => x.TypeName == plugin.TypeName && x.Version == plugin.Version))
                    {
                        _loadedPluginDefinitions.Add(plugin);
                    }
                }
            }
            return true;
        }

        private KeyValuePair<InstanceDefinition, IPlugin> FindPluginEntry(Guid internalId)
        {
            var plugin = _pluginContainer.FirstOrDefault(x => x.Key.InternalId == internalId);
            if (plugin.Key != null)
            {
                return plugin;
            }
            else
            {
                throw new Exception($"Plugin not found! (id = {internalId})");
            }
        }

        /// <summary>
        /// Visszadja az adott azonosítóval rendelkező plugin példány referenciáját
        /// </summary>
        /// <param name="internalId">Plugin azonosító</param>
        /// <returns>Plugin példány referencia</returns>
        private IPlugin FindPluginInstanceReference(Guid internalId)
        {
            var plugin = _pluginContainer.FirstOrDefault(x => x.Key.InternalId == internalId);
            if (plugin.Key != null)
            {
                return plugin.Value;
            }
            else
            {
                throw new Exception($"Plugin not found! (id = {internalId})");
            }
        }

        /// <summary>
        /// Meghív egy IPlugin Action-nek megfelleő metódus referenciát, és vár a megadoitt plugin állapot bekövetkeztére.
        ///  Ha a plugin nem jelenti vissza a PluginStatusChanged eseményen át az állapotot a timouton belül, akkor timoutra fut.
        /// </summary>
        /// <param name="pluginReference">plugin példány</param>
        /// <param name="ipluginAction">Az IPlugin metzódus hívás, amely után várunk az adott állapot bekövetkeztére</param>
        /// <param name="timeOut">ennyi idő alatt kell bekövetkeznie az állapotváltásnak</param>
        /// <param name="throwException">dobjon-e kivételt, vagy lenyelje őket</param>
        /// <returns>True, ha a várt állapot bekövetkezett timouton belül, egyébként false</returns>
        private bool CallPluginAcctionWithWaitForPluginStateChange(IPlugin pluginReference, Action ipluginAction, byte timeOut = 5, bool throwException = true)
        {
            var data = new Dictionary<string, string>();
            try
            {
                if (pluginReference == null)
                {
                    data = new Dictionary<string, string>()
                        {
                            { "IPlugin Acction", ipluginAction.GetMethodInfo().Name },
                            { "Used timout (second)", timeOut.ToString() },
                        };
                    throw new FatalException("Invalid code in ApplicationContainer class! Call PluginAcctionWithWaitForPluginStateChange without plugin reference!");
                }
                var plugin = _pluginContainer.FirstOrDefault(x => x.Key.InternalId == pluginReference.WhoAmI.InternalId);
                if (ipluginAction == null)
                {
                    data = new Dictionary<string, string>()
                        {
                            { "PluginType", plugin.Key.Type.TypeName },
                            { "Version", plugin.Key.Type.Version },
                            { "InstanceId", plugin.Key.Id },
                            { "Used timout (second)", timeOut.ToString() },
                        };
                    throw new FatalException("Invalid code in ApplicationContainer class! Call PluginAcctionWithWaitForPluginStateChange without Action reference!");
                }
                // Plugin State Check
                PluginStateEnum requestedInitialState = PluginStateEnum.Unknown;
                PluginStateEnum expectedTargetState = PluginStateEnum.Unknown;
                switch (ipluginAction.GetMethodInfo().Name)
                {
                    case "Start":
                        requestedInitialState = PluginStateEnum.Loaded;
                        expectedTargetState = PluginStateEnum.Running;
                        break;
                    case "Stop":
                        requestedInitialState = PluginStateEnum.Running;
                        expectedTargetState = PluginStateEnum.Loaded;
                        break;
                    case "Dispose":
                        requestedInitialState = PluginStateEnum.Loaded;
                        expectedTargetState = PluginStateEnum.Disposed;
                        break;
                    default:
                        data = new Dictionary<string, string>()
                        {
                            { "PluginType", plugin.Key.Type.TypeName },
                            { "Version", plugin.Key.Type.Version },
                            { "InstanceId", plugin.Key.Id },
                            { "IPlugin Acction", ipluginAction.GetMethodInfo().Name },
                            { "Used timout (second)", timeOut.ToString() },
                            { "Expected plugin state", expectedTargetState.ToString() },
                        };
                        throw new FatalException("Invalid code in ApplicationContainer class! Call PluginAcctionWithWaitForPluginStateChange with not valid IPlugin Acction reference!");
                }
                if (pluginReference.Status.State != requestedInitialState && expectedTargetState != PluginStateEnum.Disposed)
                {
                    if (throwException)
                    {
                        data = new Dictionary<string, string>()
                        {
                            { "PluginType", plugin.Key.Type.TypeName },
                            { "Version", plugin.Key.Type.Version },
                            { "InstanceId", plugin.Key.Id },
                            { "IPlugin Acction", ipluginAction.GetMethodInfo().Name },
                            { "Used timout (second)", timeOut.ToString() },
                            { "Requested initial state", requestedInitialState.ToString() },
                            { "Expected plugin state", expectedTargetState.ToString() },
                        };
                        throw new Exception($"Action not allowed when the plugin state is {pluginReference.Status.State}! " +
                            $"Accepted initial state: {requestedInitialState} only! ");
                    }
                }
                if (plugin.Key != null)
                {
                    plugin.Key.WaitForThisState = expectedTargetState;
                    plugin.Key.WaitForPluginStateChangeEvent.Reset();
                    var before = plugin.Value.Status;
                    DateTime startTimeStamp = DateTime.UtcNow;
                    ipluginAction?.BeginInvoke(null, null);
                    if (plugin.Key.WaitForPluginStateChangeEvent.WaitOne(timeOut * 1000))
                    {
                        // signaled (state change)
                        switch (ipluginAction.GetMethodInfo().Name)
                        {
                            case "Start":
                                plugin.Key.LastKnownStartTimeStamp = startTimeStamp;
                                plugin.Key.LastKnownStartTimeCost = DateTime.UtcNow.Subtract(startTimeStamp).TotalSeconds;
                                break;
                            case "Stop":
                                plugin.Key.LastKnownStopTimeStamp = startTimeStamp;
                                plugin.Key.LastKnownStopTimeCost = DateTime.UtcNow.Subtract(startTimeStamp).TotalSeconds;
                                break;
                            case "Dispose":
                                plugin.Key.LastKnownDisposeTimeStamp = startTimeStamp;
                                plugin.Key.LastKnownDisposeTimeCost = DateTime.UtcNow.Subtract(startTimeStamp).TotalSeconds;
                                break;
                            default:
                                break;
                        }
                        return true;
                    }
                    else
                    {
                        // timout
                        data = new Dictionary<string, string>()
                        {
                            { "PluginType", plugin.Key.Type.TypeName },
                            { "Version", plugin.Key.Type.Version },
                            { "InstanceId", plugin.Key.Id },
                            { "IPlugin Acction", ipluginAction.GetMethodInfo().Name },
                            { "Used timout (second)", timeOut.ToString() },
                            { "Expected plugin state", expectedTargetState.ToString() },
                        };
                        LogThis("WaitPluginState Timout!", data, null, LogLevel.Error);
                        return false;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                LogThis("Error in PluginContainer Acction!", data, ex, ex is FatalException ? LogLevel.Fatal : LogLevel.Error);
                if (throwException)
                {
                    throw ex;
                }
                else
                {
                    return false;
                }
            }
        }

        private void Config_ConfigProcessorEvent(ConfigProcessorEventArgs e)
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
        /// Betölti az instance factory plugint
        /// </summary>
        private void LoadInstanceFactoryPlugin()
        {
            lock (_instanceLocker)
            {
                string appDir = string.Empty;
                string assemblyPattern = _config.InstanceFactoryAssembly;
                string version = _config.InstanceFactoryVersion;
                string concrateType = _config.InstanceFactoryType;
                try
                {
                    RegistrationBuilder registration = new RegistrationBuilder();
                    registration.ForType<IInstanceFactory>().ExportInterfaces();
                    appDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                    if (string.IsNullOrEmpty(assemblyPattern))
                    {
                        assemblyPattern = "*.dll";
                    }
                    var d = new DirectoryInfo(appDir);
                    FileInfo[] files = d.GetFiles(assemblyPattern);
                    using (AggregateCatalog catalog = new AggregateCatalog())
                    {
                        if (files.Count() == 0)
                        {
                            // TODO: ML
                            throw new FatalException("Nem található a feltételnek megfelelő dll az alakalmazás könyvtárában!", null
                                                        , new KeyValuePair<string, string>("Application directory", appDir)
                                                        , new KeyValuePair<string, string>("AssemblyPattern", assemblyPattern)
                                                        );
                        }
                        foreach (FileInfo file in files)
                        {
                            Assembly assembly = Assembly.LoadFrom(file.FullName);
                            bool addThis = true;
                            if (!string.IsNullOrEmpty(concrateType))
                            {
                                addThis = assembly.ExportedTypes.Any(x => x.Name == concrateType || x.FullName == concrateType);
                            }
                            if (!string.IsNullOrEmpty(version))
                            {
                                addThis = addThis && FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion == version;
                            }
                            if (addThis)
                            {
                                catalog.Catalogs.Add(new AssemblyCatalog(file.FullName));
                            }
                        }
                        _container = new CompositionContainer(catalog, CompositionOptions.DisableSilentRejection | CompositionOptions.IsThreadSafe);
                        if (_container.Catalog.Count() == 0)
                        {
                            // TODO: ML
                            throw new FatalException("Nem található a feltételeknek megfellelő InstanceFactory plugin!", null
                                                        , new KeyValuePair<string, string>("Application directory", appDir)
                                                        , new KeyValuePair<string, string>("AssemblyPattern", assemblyPattern)
                                                        , new KeyValuePair<string, string>("Conrate Type", concrateType)
                                                        , new KeyValuePair<string, string>("Version", version)
                                );
                        }
                        _container.ComposeParts(this);
                    }
                }
                catch (FatalException ex)
                {
                    LogThis("Fatal Exception oocured: Application contianer not work!", null, ex, LogLevel.Fatal);
                }
                catch (Exception ex)
                {
                    //TODO: ML
                    var fatalex = new FatalException("Hiba történt az InstanceFactory plugin betöltése közben.", ex
                                                    , new KeyValuePair<string, string>("Application directory", appDir)
                                                    , new KeyValuePair<string, string>("AssemblyPattern", assemblyPattern)
                                                    , new KeyValuePair<string, string>("Conrate Type", concrateType)
                                                    , new KeyValuePair<string, string>("Version", version)
                                        );
                    LogThis("Fatal Exception oocured: Application contianer not work!", null, fatalex, LogLevel.Fatal);
                }
            }
            }

        private List<PluginDefinition> GetAllPluginDefinition()
        {
            List<PluginDefinition> plugins = new List<PluginDefinition>();
            lock (_instanceLocker)
            {
                Dictionary<string, string> data;
                foreach (var plugin in _usedInstanceFactory.GetAllPlugin())
                {
                    if (plugins.Any(x => x.TypeName == plugin.TypeName && x.Version == plugin.Version))
                    {
                        data = new Dictionary<string, string>()
                        {
                            { "Assembly", plugin.Assembly },
                            { "Version", plugin.Version },
                            { "Plugin directory", plugin.PluginDirectory },
                        };
                        LogThis($"Configuration issue! Plugin {plugin.TypeName} with this version {plugin.Version} allready defined!", data, null, LogLevel.Information);
                        continue;
                    }
                    data = new Dictionary<string, string>()
                    {
                        { "Assembly", plugin.Assembly },
                        { "Version", plugin.Version },
                        { "Plugin directory", plugin.PluginDirectory },
                        { "Plugin level config", plugin.PluginConfig },
                        { "Auto start", plugin.AutoStart.ToString() },
                        { "Singletone", plugin.Singletone.ToString() },
                    };
                    LogThis($"Plugin found: {plugin.TypeName}", data, null, LogLevel.Information);
                    string pluginAssembly;
                    if (string.IsNullOrEmpty(plugin.PluginDirectory) || plugin.PluginDirectory.Substring(0, 1) != "@") // If start with @ sign: the name included path
                    {
                        pluginAssembly = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + Path.DirectorySeparatorChar + plugin.PluginDirectory;
                    }
                    else
                    {
                        pluginAssembly = plugin.PluginDirectory.TrimStart('@');
                    }
                    pluginAssembly = pluginAssembly + Path.DirectorySeparatorChar + plugin.Assembly;
                    var dir = new DirectoryInfo(Path.GetDirectoryName(pluginAssembly));
                    if (!dir.Exists)
                    {
                        data = new Dictionary<string, string>()
                    {
                        { "Defined plugin directory", Path.GetDirectoryName(pluginAssembly) },
                    };
                        LogThis($"{plugin.TypeName} type plugins not loaded!", data, new Exception("Defined plugin directory not found!"), LogLevel.Fatal);
                        continue;
                    }
                    else
                    {
                        FileInfo[] files = dir.GetFiles(Path.GetFileName(pluginAssembly));
                        using (AggregateCatalog catalog = new AggregateCatalog())
                        {
                            if (files.Count() == 0)
                            {
                                data = new Dictionary<string, string>()
                            {
                                { "Assembly full path", pluginAssembly },
                            };
                                LogThis($"{plugin.TypeName} type plugins not loaded!", data, new Exception("Plugin assembly is not exists!"), LogLevel.Fatal);
                                continue;
                            }
                            else
                            {
                                var file = files.FirstOrDefault();
                                Assembly assembly = Assembly.LoadFrom(file.FullName);
                                Type pluginType = assembly.GetType(plugin.TypeName);
                                plugin.Type = pluginType;
                                if (pluginType == null)
                                {
                                    data = new Dictionary<string, string>()
                                {
                                    { "Assembly full path", pluginAssembly },
                                    { "Specified type", plugin.TypeName },
                                };
                                    LogThis($"{plugin.TypeName} type plugins not loaded!", data, new Exception("Plugin assembly is not contains the specified type!"), LogLevel.Fatal);
                                    continue;
                                }
                                else
                                {
                                    if (FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion != plugin.Version)
                                    {
                                        data = new Dictionary<string, string>()
                                    {
                                        { "Assembly full path", pluginAssembly },
                                        { "Specified version", plugin.Version },
                                        { "Assembly version", FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion },
                                    };
                                        LogThis($"{plugin.TypeName} type plugins not loaded!", data, new Exception("Wrong Plugin version!"), LogLevel.Fatal);
                                        continue;
                                    }
                                    else
                                    {
                                        Type pluginInterface = pluginType.GetInterface(typeof(IPlugin).Name);
                                        if (pluginInterface == null)
                                        {
                                            data = new Dictionary<string, string>()
                                    {
                                        { "Assembly full path", pluginAssembly },
                                        { "Specified type", plugin.TypeName },
                                    };
                                            LogThis($"{plugin.TypeName} type plugins not loaded!", data, new Exception("Type is not implement the IPlugin interface!"), LogLevel.Fatal);
                                            continue;
                                        }
                                        else
                                        {
                                            plugins.Add(plugin);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return plugins;
        }

        /// <summary>
        /// Felszedi a plugin definicióhoz tartozó összes példány definiciót
        /// </summary>
        /// <param name="plugin">plugin típus leíró</param>
        /// <returns>a plugin  típus alá tartozó példányok listája</returns>
        public IEnumerable<InstanceDefinition> GetAllDefinedInstances(PluginDefinition plugin)
        {
            IEnumerable<InstanceDefinition> instances;
            if (!plugin.Singletone)
            {
                MethodInfo getInstancesMethod = plugin.Type.GetMethod("GetInstances", BindingFlags.Public | BindingFlags.Static);
                if (getInstancesMethod != null)
                {
                    try
                    {
                        instances = (IEnumerable<InstanceDefinition>)getInstancesMethod.Invoke(null, null);
                    }
                    catch (Exception ex)
                    {
                        var data = new Dictionary<string, string>()
                                                    {
                                                        { "Assembly full path", plugin.Assembly },
                                                        { "Plugin type", plugin.TypeName },
                                                        { "Plugin version", plugin.Version },
                                                    };
                        LogThis("Error in Plugin GetInstances call!", data, ex, LogLevel.Error);
                        instances = _usedInstanceFactory.GetAllInstance(plugin.TypeName, plugin.Version);
                    }
                }
                else
                {
                    instances = _usedInstanceFactory.GetAllInstance(plugin.TypeName, plugin.Version);
                }
            }
            else
            {
                InstanceDefinition instance = new InstanceDefinition
                {
                    Id = "Singletone",
                    Name = $"Singletone instance of {plugin.TypeName}",
                    Description = $"Singletone instance of {plugin.TypeName}, version: {plugin.Version}",
                    InstanceConfig = string.Empty,
                    Type = plugin
                };
                var instanceList = new List<InstanceDefinition>
                {
                    instance
                };
                instances = instanceList;
            }
            foreach (var instance in instances)
            {
                instance.Type = plugin;
            }
            return instances;
        }

        /// <summary>
        /// Üzembe helyezi a plugin példányt (példányosítja, és ha autostart, elindítja)
        /// </summary>
        /// <param name="instance"></param>
        private void StartUpInstance(InstanceDefinition instance)
        {
            lock (_instanceLocker)
            {
                var data = new Dictionary<string, string>();
                if (_pluginContainer.Any(x => x.Key.Id == instance.Id))
                {
                    data = new Dictionary<string, string>()
                        {
                            { "Instance Id", instance.Id },
                            { "Name", instance.Name },
                            { "Description", instance.Description },
                            { "Instance level config", instance.InstanceConfig },
                            { "Instance data", instance.InstanceData?.ToString() }
                        };
                    LogThis($"Instance allready exists: {instance.Id}", data, null, LogLevel.Error);
                    return;
                }
                data = new Dictionary<string, string>()
                        {
                            { "Name", instance.Name },
                            { "Description", instance.Description },
                            { "Instance level config", instance.InstanceConfig },
                            { "Instance data", instance.InstanceData?.ToString() }
                        };
                LogThis($"Instance found: {instance.Id}", data, null, LogLevel.Information);
                string factory = instance.Type.FactoryMethodName;
                if (string.IsNullOrEmpty(factory))
                {
                    factory = $"{instance.Type.Type.Name}Factory";
                }
                MethodInfo factoryMethod = instance.Type.Type.GetMethod(factory, BindingFlags.Public | BindingFlags.Static);
                IPlugin pluginInstance = null;
                try
                {
                    if (factoryMethod != null)
                    {
                        object[] parameters = new object[] { (object)instance, (object)instance.InstanceData };
                        DateTime loadStart = DateTime.UtcNow;
                        pluginInstance = (IPlugin)factoryMethod.Invoke(null, parameters);
                        instance.LastKnownLoadTimeStamp = loadStart;
                        instance.LastKnownLoadTimeCost = DateTime.UtcNow.Subtract(loadStart).TotalSeconds;
                    }
                    else
                    {
                        data = new Dictionary<string, string>()
                                                {
                                                    { "Plugin type", instance.Type.TypeName },
                                                    { "Plugin version", instance.Type.Version },
                                                    { "Instance id", instance.Id },
                                                    { "Specified Factory", factory },
                                                };
                        LogThis($"Specified factory ({factory}) not found! Try use parameterles public constructor.",
                            data, null, LogLevel.Warning);
                        var constructors = instance.Type.Type.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
                        var pars = constructors[0].GetParameters();
                        ConstructorInfo ctor = constructors.FirstOrDefault(x => x.GetParameters().Count() == 0);
                        if (ctor != null)
                        {
                            DateTime loadStart = DateTime.UtcNow;
                            pluginInstance = (IPlugin)ctor.Invoke(null);
                            instance.LastKnownLoadTimeStamp = loadStart;
                            instance.LastKnownLoadTimeCost = DateTime.UtcNow.Subtract(loadStart).TotalSeconds;
                        }
                        else
                        {
                            data = new Dictionary<string, string>()
                                                {
                                                    { "Plugin type", instance.Type.TypeName },
                                                    { "Plugin version", instance.Type.Version },
                                                    { "Instance id", instance.Id },
                                                };
                            LogThis($"Constructor not found! Plugin instance not created! (Factory: {factory})", data, null, LogLevel.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    data = new Dictionary<string, string>()
                                                {
                                                    { "Plugin type", instance.Type.TypeName },
                                                    { "Plugin version", instance.Type.Version },
                                                    { "Instance id", instance.Id },
                                                    { "Used factory", factoryMethod != null ? factoryMethod.Name : "Constructor" }
                                                };
                    LogThis($"Plugin not loaded! Error in Plugin instantiation!", data, ex, LogLevel.Fatal);
                }
                if (pluginInstance != null)
                {
                    data = new Dictionary<string, string>()
                                                {
                                                    { "Plugin type", instance.Type.TypeName },
                                                    { "Plugin version", instance.Type.Version },
                                                    { "Instance id", instance.Id },
                                                    { "Factory methode", factoryMethod != null ? factoryMethod.Name : "Constructor" }, 
                                                };
                    LogThis("Plugin instance loaded!", data, null, LogLevel.Information);
                    _pluginContainer.Add(instance, pluginInstance);
                    pluginInstance.WhoAmI = instance;
                    pluginInstance.PluginStatusChanged += PluginInstance_PluginStatusChanged;
                    if (instance.Type.AutoStart)
                    {
                        Task.Run(() =>
                            CallPluginAcctionWithWaitForPluginStateChange(pluginInstance, pluginInstance.Start)
                            );
                    }
                }
            }
        }

        /// <summary>
        /// Plugin 
        /// </summary>
        /// <param name="pluginId"></param>
        /// <param name="state"></param>
        private void PluginInstance_PluginStatusChanged(Guid pluginId, PluginStateEnum state)
        {
            var plugin = _pluginContainer.FirstOrDefault(x => x.Key.InternalId == pluginId);
            if (plugin.Key != null)
            {
                lock (_instanceLocker)
                {
                    if (plugin.Key.WaitForThisState == state || state == PluginStateEnum.Disposed || (state == PluginStateEnum.Disposing && plugin.Key.WaitForThisState != PluginStateEnum.Disposed) || state == PluginStateEnum.Error)
                    {
                        plugin.Key.WaitForPluginStateChangeEvent.Set();
                    }
                    if (state == PluginStateEnum.Disposed)
                    {
                        plugin.Value.PluginStatusChanged -= PluginInstance_PluginStatusChanged;
                        _pluginContainer.Remove(plugin.Key);
                    }
                }
            }
        }

        /// <summary>
        /// Elindítja a szolgáltatást
        /// </summary>
        /// <param name="wcfhostdescriptor">egy connectionstring store elem neve</param>
        private void StartService(ConnectionStringStore.WCFHostDescriptor wcfhostdescriptor)
        {
            try
            {
                lock (_instanceLocker)
                {
                    StopService();
                    _service = wcfhostdescriptor.WcfHost;
                    _service.Open();
                    StringBuilder sb = new StringBuilder();
                    foreach (var baseAddress in _service.BaseAddresses)
                    {
                        sb.AppendLine(baseAddress.AbsoluteUri);
                    }
                    var data = new Dictionary<string, string>()
                        {
                            { "BaseAddresses", sb.ToString() },
                            { "State", _service.State.ToString() },
                        };
                    LogThis("Application service host started and opened!", data, null, LogLevel.Information);
                }
            }
            catch (Exception ex)
            {
                LogThis("Error occured in start Application host service!", null, ex, LogLevel.Error);
            }
        }

        private void StopService()
        {
            try
            {
                if (_service != null)
                {
                    _service.Close();
                    _service = null;
                }
            }
            catch (Exception ex)
            {
                LogThis("Error occured in stop Application host service!", null, ex, LogLevel.Error);
            }
        }

        private void GetPartInfo(ComposablePartCatalog catalog)
        {
            Console.WriteLine("No of parts: {0}", catalog.Count());
            int i = 0;
            foreach (var part in catalog)
            {
                //Console.WriteLine(JsonConvert.SerializeObject(part));
                i++;
                Console.WriteLine("Part {0}:", i);
                Console.WriteLine("\tNo of Exports in this parts: {0}", part.ExportDefinitions.Count());

                int j = 0;
                foreach (var export in part.ExportDefinitions)
                {
                    j++;
                    Console.WriteLine("\t\tExport {0}:", j);
                    Console.WriteLine("\t\t{0}", (export.Metadata["ConcrateType"] as Type).FullName);
                    Console.WriteLine("\t\t{0}", FileVersionInfo.GetVersionInfo((export.Metadata["ConcrateType"] as Type).Assembly.Location).ProductVersion);
                    Console.WriteLine("\t\t{0}", (export.Metadata["ConcrateType"] as Type).Assembly.Location);
                    Console.WriteLine("\t\t{0}", JsonConvert.SerializeObject(export));
                    Console.WriteLine("\t\t{0}", Assembly.GetAssembly((export.Metadata["ConcrateType"] as Type)).CodeBase);

                    //Console.WriteLine(Assembly.Load(export..ContractName).GetType());                    
                    //Console.WriteLine(JsonConvert.SerializeObject(exp));
                }
            }
        }

        private void LogThis(string message, Dictionary<string, string> data, Exception ex, LogLevel level, [CallerMemberName]string caller = "", [CallerLineNumber]int line = 0)
        {
            VrhLogger.Log<string>(message, data, ex, level, this.GetType(), caller, line);
            MessageStackEntry e = new MessageStackEntry()
            {
                Body = message,
                Data = data,
                TimeStamp = DateTime.UtcNow,
            };
            switch (level)
            {
                case LogLevel.Information:
                case LogLevel.Warning:
                    if (ex != null)
                    {
                        if (e.Data == null)
                        {
                            e.Data = new Dictionary<string, string>();
                        }
                        e.Data.Add("Exception", Logger.LogHelper.GetExceptionInfo(ex));
                    }
                    e.Type = level == LogLevel.Warning ? Level.Warning : Level.Info;
                    lock (_infoStack)
                    {
                        _infoStack.DropItem(e);
                    }
                    break;
                case LogLevel.Error:
                case LogLevel.Fatal:
                    if (ex != null)
                    {
                        if (e.Data == null)
                        {
                            e.Data = new Dictionary<string, string>();
                        }
                        e.Data.Add("Exception", Logger.LogHelper.GetExceptionInfo(ex));
                    }
                    e.Type = level == LogLevel.Fatal ? Level.FatalError : Level.Error;
                    lock (_errorStack)
                    {
                        _errorStack.DropItem(e);
                    }
                    break;
                default:
                    return;
            }
        }

#pragma warning disable 0649
        [Import]
#pragma warning disable IDE0044 // Add readonly modifier: This is a fake Warning 'cause DI over MEF!!!
        private IInstanceFactory _usedInstanceFactory;
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore 0649

        private CompositionContainer _container;

        private readonly ApplicationContainerConfig _config;

        private readonly Dictionary<InstanceDefinition, IPlugin> _pluginContainer = new Dictionary<InstanceDefinition, IPlugin>();

        private readonly List<PluginDefinition> _loadedPluginDefinitions = new List<PluginDefinition>();

#pragma warning disable IDE0069 // Disposable fields should be disposed: This is a mistaken Warning. This is sisposed across multiple methathesis... 
        private ServiceHost _service;
#pragma warning restore IDE0069 // Disposable fields should be disposed

        private readonly ApplicationContainerService _wcfServiceInstance;

        private readonly object _instanceLocker = new object();

        private readonly DateTime _startupTimeStamp;

        private TimeSpan _lastStartupCost;

        /// <summary>
        /// Verem az utolsó X db hibával kapcsolatos információ tárolására
        /// </summary>
        private readonly FixStack<MessageStackEntry> _errorStack = new FixStack<MessageStackEntry>();

        /// <summary>
        /// Verem az utolsó X db működéssel kapcsolatos információ tárolására
        /// </summary>
        private readonly FixStack<MessageStackEntry> _infoStack = new FixStack<MessageStackEntry>();

        /// <summary>
        /// Modul azonosító
        /// </summary>
        internal const string MODULEPREFIX = "Vrh.ApplicationContainer";
        
        /// <summary>
        /// A hazsnált config fájlt definiáló app settings elem neve
        /// </summary>
        internal const string CONFIGURATIONFILE_ELEMENT_NAME = "ConfigurationFile";

        /// <summary>
        /// A WCF alapcímét definiáló appsettings kulcs
        /// </summary>
        internal const string WCFBASEADDRESS_ELEMENT_NAME = "WCFBaseAddressList";

        /// <summary>
        /// Visszadja az application config kulcs nevet (modul prefix + kulcs)
        /// </summary>
        /// <param name="key">beállítás kulcs</param>
        /// <returns>erős név kulcs</returns>
        internal static string GetApplicationConfigName(string key)
        {
            return $"{ApplicationContainer.MODULEPREFIX}:{key}";           
        }


        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Dispose implementáció
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    StopService();
                    // TODO: dispose managed state (managed objects).
                    if (_config != null)
                    {
                        _config.ConfigProcessorEvent -= Config_ConfigProcessorEvent;
                        _config.Dispose();
                    }
                    var pluginItem = _pluginContainer.FirstOrDefault();
                    while (pluginItem.Key != null)
                    {
                        CallPluginAcctionWithWaitForPluginStateChange(pluginItem.Value, pluginItem.Value.Dispose, throwException: false);
                        lock (_instanceLocker)
                        {
                            pluginItem = _pluginContainer.FirstOrDefault();
                        }
                    }
                    _container?.Dispose();                   
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ApplicationContainer() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }


        /// <summary>
        /// This code added to correctly implement the disposable pattern.
        /// </summary>
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
