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
using Newtonsoft.Json;
using System.Configuration;
using Vrh.Logger;
using System.ServiceModel;
using VRH.Common;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Diagnostics.Runtime;

namespace Vrh.ApplicationContainer
{
    public class ApplicationContainer : IDisposable
    {
        public ApplicationContainer()
        {
            _startupTimeStamp = DateTime.UtcNow;
            _wcfServiceInstance.ApplicationContainerReference = this;
            string configFile = ConfigurationManager.AppSettings[_configFileSettingKey];
            if (String.IsNullOrEmpty(configFile))
            {
                configFile = @"ApplicationContainer.Config.xml";
            }
            _config = new ApplicationContainerConfig(configFile);
            _config.ConfigProcessorEvent += _config_ConfigProcessorEvent;
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
                        Task.Run(() => StartUpInstance(instance));
                    }
                }
            }
            StartService();
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
            LogThis(String.Format("Vrh.ApplicationContainer {0} started.", this.GetType().Assembly.Version()), data, null, LogLevel.Information);
        }

        private void TestDiagnostic()
        {
            ulong address = 0;
            unsafe
            {
                Object o = (Object)_pluginContainer.FirstOrDefault().Value;
                TypedReference tr = __makeref(o);
                IntPtr ptr = **(IntPtr**)(&tr);
                address = (ulong)ptr;
            }
            using (DataTarget target = DataTarget.AttachToProcess(
                Process.GetCurrentProcess().Id, 5000, AttachFlag.Passive))
            {
                ClrRuntime runtime = target.ClrVersions.First().CreateRuntime();
                //foreach (var item in runtime.AppDomains)
                //{
                //    Console.WriteLine("Appdomain info:");
                //    Console.WriteLine("Name: {0}\nAddress: {1}\n {2},ApplicationBase: {3}\nId: {4}",
                //    item.Name, item.Address, item.ApplicationBase, item.ConfigurationFile, item.Id);
                //    foreach (var module in item.Modules)
                //    {
                //        //Console.WriteLine(module.AssemblyName);
                //    }
                //}
                ClrHeap heap = runtime.GetHeap();
                foreach (var item in runtime.Threads)
                {
                    Console.WriteLine(item.OSThreadId);
                }
                var c = Process.GetCurrentProcess();
                var t = c.Threads;
                foreach (ProcessThread item in t)
                {
                    if (item.TotalProcessorTime == new TimeSpan(0, 0, 0))
                    {
                        continue;
                    }
                    Console.WriteLine("__________________________________");
                    ClrThread clrT = runtime.Threads.FirstOrDefault(x => x.OSThreadId == item.Id);
                    if (clrT != null)
                    {
                        Console.WriteLine(clrT.EnumerateStackTrace().FirstOrDefault()?.DisplayString);
                        Console.WriteLine(clrT.EnumerateStackTrace().LastOrDefault()?.DisplayString);
                    }
                    //Console.WriteLine();
                    Console.WriteLine("ID: {0} StartTime: {1} State: {2} TotalPT: {3} UserPT: {4}, PrivilegedPT: {5} WR: {6}"
                        , item.Id, item.StartTime, item.ThreadState, item.TotalProcessorTime, item.UserProcessorTime, item.PrivilegedProcessorTime, item.ThreadState == System.Diagnostics.ThreadState.Wait ? item.WaitReason.ToString() : "");
                }
                Console.WriteLine(heap.TotalHeapSize);
                ClrType type = heap.GetObjectType(address);
                foreach (var item in type.Fields)
                {
                    Console.WriteLine("{0}: {1}", item.Name, item.GetAddress(address));
                }

                Console.WriteLine("{0}: {1}, {2}", type.Name, type.GetSize(address), type.IsFinalizable);



                //if (!heap.CanWalkHeap)
                //{
                //    Console.WriteLine("Cannot walk the heap!");
                //}
                //else
                //{
                //    foreach (var item in heap.Segments)
                //    {
                //        Console.WriteLine(item.Length);
                //        foreach (ulong obj in item.EnumerateObjectAddresses())
                //        {                                
                //            ClrType type = heap.GetObjectType(obj);
                //            if (type == null)
                //                continue;
                //            if (type.Name.Contains("System.Collections.Generic.Dictionary<Vrh.ApplicationContainer.InstanceDefinition,Vrh.ApplicationContainer.IPlugin"))
                //            {                                    
                //                Console.WriteLine("{0}: {1}", type.Name, type.GetSize(obj));
                //            }
                //        }
                //    }
                //    //foreach (ClrType item in heap.EnumerateTypes().OrderBy(x => x.Name))
                //    //{
                //    //    if (item.Name.Contains("Plugin"))
                //    //        Console.WriteLine(item.Name);
                //    //}
                //    //foreach (ulong obj in heap.EnumerateObjectAddresses())
                //    //{
                //    //    get type = heap.GetObjectType(obj);

                //    //    // If heap corruption, continue past this object.
                //    //    if (type == null)
                //    //        continue;

                //    //    ulong size = type.GetSize(obj);
                //    //    Console.WriteLine("{0,12:X} {1,8:n0} {2,1:n0} {3}", obj, size, heap.GetObjectGeneration(obj), type.Name);
                //    //}
                //}
            }
        }

        public ApplicationContainerInfo MyInfo
        {
            get
            {
                TestDiagnostic();
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

        public List<InstanceDefinition> GetInstances(string pluginType, string version)
        {
            List<InstanceDefinition> instances = new List<InstanceDefinition>();
            foreach (var item in _pluginContainer.Where(x => x.Key.Type.TypeName == pluginType && x.Key.Type.Version == version))
            {
                instances.Add(item.Key);
            }
            return instances;
        }

        public PluginStatus GetPluginStatus(Guid internalId)
        {
            return FindPluginInstanceReference(internalId).Status;
        }

        public List<MessageStackEntry> GetPluginInstanceErrors(Guid internalId)
        {
            return FindPluginInstanceReference(internalId).Errors;
        }

        public List<MessageStackEntry> GetPluginInstanceInfos(Guid internalId)
        {
            return FindPluginInstanceReference(internalId).Infos;
        }

        public bool StartPlugin(Guid internalId)
        {
            var pluginReference = FindPluginInstanceReference(internalId);
            return CallPluginAcctionWithWaitForPluginStateChange(pluginReference, pluginReference.Start);
        }

        public bool StopPlugin(Guid internalId)
        {
            var pluginReference = FindPluginInstanceReference(internalId);
            return CallPluginAcctionWithWaitForPluginStateChange(pluginReference, pluginReference.Stop);
        }

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
                throw new Exception(String.Format("Plugin not found! (id = {0})", internalId));
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
                throw new Exception(String.Format("Plugin not found! (id = {0})", internalId));
            }
        }

        /// <summary>
        /// Meghív egy IPlugin Action-nek megfelleő metódus referenciát, és vár a megadoitt plugin állapot bekövetkeztére.
        ///  Ha a plugin nem jelenti vissza a PluginStatusChanged eseményen át az állapotot a timouton belül, akkor timoutra fut.
        /// </summary>
        /// <param name="pluginReference">plugin példány</param>
        /// <param name="targetState">várt állapot</param>
        /// <param name="ipluginAction">Az IPlugin metzódus hívás, amely után várunk az adott állapot bekövetkeztére</param>
        /// <param name="timeOut">timout</param>
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
                        throw new Exception(String.Format("Action not allowed when the plugin state is {0}! Accepted initial state: {1} only! ",
                                pluginReference.Status.State, requestedInitialState
                            ));
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

        private void _config_ConfigProcessorEvent(LinqXMLProcessor.Base.ConfigProcessorEventArgs e)
        {
            LogLevel level =
                e.Exception.GetType().Name == typeof(Vrh.LinqXMLProcessor.Base.ConfigProcessorWarning).Name
                    ? LogLevel.Warning
                    : LogLevel.Error;
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "ConfigProcessor class", e.ConfigProcessor },
                { "Config file", e.ConfigFile },
            };
            LogThis(String.Format("Configuration issue: {0}", e.Message), data, e.Exception, level);
        }

        /// <summary>
        /// Betölti az instance factory plugint
        /// </summary>
        private void LoadInstanceFactoryPlugin()
        {
            lock (_instanceLocker)
            {
                string appDir = String.Empty;
                string assemblyPattern = _config.InstanceFactoryAssembly;
                string version = _config.InstanceFactoryVersion;
                string concrateType = _config.InstanceFactoryType;
                try
                {
                    RegistrationBuilder registration = new RegistrationBuilder();
                    registration.ForType<IInstanceFactory>().ExportInterfaces();
                    appDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                    if (String.IsNullOrEmpty(assemblyPattern))
                    {
                        assemblyPattern = "*.dll";
                    }
                    DirectoryInfo d = new DirectoryInfo(appDir);
                    FileInfo[] files = d.GetFiles(assemblyPattern);
                    AggregateCatalog catalog = new AggregateCatalog();
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
                        if (!String.IsNullOrEmpty(concrateType))
                        {
                            addThis = assembly.ExportedTypes.Any(x => x.Name == concrateType || x.FullName == concrateType);
                        }
                        if (!String.IsNullOrEmpty(version))
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
                catch (FatalException ex)
                {
                    LogThis("Fatal Exception oocured: Application contianer not work!", null, ex, LogLevel.Fatal);
                    //throw ex;
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
                    //throw fatalex;
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
                        LogThis(String.Format("Configuration issue! Plugin {0} with this version {1} allready defined!", plugin.TypeName, plugin.Version), data, null, LogLevel.Information);
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
                    LogThis(String.Format("Plugin found: {0}", plugin.TypeName), data, null, LogLevel.Information);
                    string pluginAssembly;
                    if (String.IsNullOrEmpty(plugin.PluginDirectory) || plugin.PluginDirectory.Substring(0, 1) != "@") // If start with @ sign: the name included path
                    {
                        pluginAssembly = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + Path.DirectorySeparatorChar + plugin.PluginDirectory;
                    }
                    else
                    {
                        pluginAssembly = plugin.PluginDirectory.TrimStart('@');
                    }
                    pluginAssembly = pluginAssembly + Path.DirectorySeparatorChar + plugin.Assembly;
                    DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(pluginAssembly));
                    if (!dir.Exists)
                    {
                        data = new Dictionary<string, string>()
                    {
                        { "Defined plugin directory", Path.GetDirectoryName(pluginAssembly) },
                    };
                        LogThis(String.Format("{0} type plugins not loaded!", plugin.TypeName), data, new Exception("Defined plugin directory not found!"), LogLevel.Fatal);
                        continue;
                    }
                    else
                    {
                        FileInfo[] files = dir.GetFiles(Path.GetFileName(pluginAssembly));
                        AggregateCatalog catalog = new AggregateCatalog();
                        if (files.Count() == 0)
                        {
                            data = new Dictionary<string, string>()
                            {
                                { "Assembly full path", pluginAssembly },
                            };
                            LogThis(String.Format("{0} type plugins not loaded!", plugin.TypeName), data, new Exception("Plugin assembly is not exists!"), LogLevel.Fatal);
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
                                LogThis(String.Format("{0} type plugins not loaded!", plugin.TypeName), data, new Exception("Plugin assembly is not contains the specified type!"), LogLevel.Fatal);
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
                                    LogThis(String.Format("{0} type plugins not loaded!", plugin.TypeName), data, new Exception("Wrong Plugin version!"), LogLevel.Fatal);
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
                                        LogThis(String.Format("{0} type plugins not loaded!", plugin.TypeName), data, new Exception("Type is not implement the IPlugin interface!"), LogLevel.Fatal);
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
            return plugins;
        }

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
                InstanceDefinition instance = new InstanceDefinition();
                instance.Id = "Singletone";
                instance.Name = String.Format("Singletone instance of {0}", plugin.TypeName);
                instance.Description = String.Format("Singletone instance of {0}, version: {1}", plugin.TypeName, plugin.Version);
                instance.InstanceConfig = String.Empty;
                instance.Type = plugin;
                List<InstanceDefinition> instanceList = new List<InstanceDefinition>();
                instanceList.Add(instance);
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
                    LogThis(String.Format("Instance allready exists: {0}", instance.Id), data, null, LogLevel.Error);
                    return;
                }
                data = new Dictionary<string, string>()
                        {
                            { "Name", instance.Name },
                            { "Description", instance.Description },
                            { "Instance level config", instance.InstanceConfig },
                            { "Instance data", instance.InstanceData?.ToString() }
                        };
                LogThis(String.Format("Instance found: {0}", instance.Id), data, null, LogLevel.Information);
                string factory = instance.Type.FactoryMethodName;
                if (String.IsNullOrEmpty(factory))
                {
                    factory = String.Format("{0}Factory", instance.Type.Type.Name);
                }
                MethodInfo factoryMethod = instance.Type.Type.GetMethod(factory, BindingFlags.Public | BindingFlags.Static);
                IPlugin pluginInstance = null;
                try
                {
                    if (factoryMethod != null)
                    {
                        Object[] parameters = new Object[] { (Object)instance, (Object)instance.InstanceData };
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
                        LogThis(String.Format("Specified factory ({0}) not found! Try use parameterles public constructor.", factory),
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
                            LogThis(String.Format("Constructor not found! Plugin instance not created!", factory), data, null, LogLevel.Error);
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
                    LogThis(String.Format("{0} type plugin not loaded! Error in Plugin instantiation with {1}!", instance.Type.TypeName, factoryMethod != null ? factoryMethod.Name : "Constructor"),
                        data, ex, LogLevel.Fatal);
                }
                if (pluginInstance != null)
                {
                    data = new Dictionary<string, string>()
                                                {
                                                    { "Plugin type", instance.Type.TypeName },
                                                    { "Plugin version", instance.Type.Version },
                                                    { "Instance id", instance.Id },
                                                };
                    LogThis(String.Format("Plugin instance loaded!", instance.Type.TypeName, factoryMethod != null ? factoryMethod.Name : "Constructor"),
                        data, null, LogLevel.Information);
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

        private void StartService()
        {
            try
            {
                lock (_instanceLocker)
                {
                    StoptService();
                    _service = new ServiceHost(_wcfServiceInstance);
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
                    LogThis(String.Format("Application service host started and opened!"), data, null, LogLevel.Information);
                }
            }
            catch (Exception ex)
            {
                LogThis(String.Format("Error occured in start Application host service!"), null, ex, LogLevel.Error);
            }
        }

        private void StoptService()
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
                LogThis(String.Format("Error occured in stop Application host service!"), null, ex, LogLevel.Error);
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
            Logger.Logger.Log<string>(message, data, ex, level, this.GetType(), caller, line);
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

        [Import]
        private IInstanceFactory _usedInstanceFactory;

        private CompositionContainer _container;

        private ApplicationContainerConfig _config;

        private Dictionary<InstanceDefinition, IPlugin> _pluginContainer = new Dictionary<InstanceDefinition, IPlugin>();

        private List<PluginDefinition> _loadedPluginDefinitions = new List<PluginDefinition>();

        private ServiceHost _service;

        private ApplicationContainerService _wcfServiceInstance = new ApplicationContainerService();

        private string _configFileSettingKey
        {
            get
            {
                return String.Format("{0}:{1}", MODULEPREFIX, "ConfigurationFile");
            }
        }

        private Object _instanceLocker = new Object();

        private DateTime _startupTimeStamp;

        private TimeSpan _lastStartupCost;

        /// <summary>
        /// Verem az utolsó X db hibával kapcsolatos információ tárolására
        /// </summary>
        private FixStack<MessageStackEntry> _errorStack = new FixStack<MessageStackEntry>();

        /// <summary>
        /// Verem az utolsó X db működéssel kapcsolatos információ tárolására
        /// </summary>
        private FixStack<MessageStackEntry> _infoStack = new FixStack<MessageStackEntry>();

        /// <summary>
        /// Modul azonosító
        /// </summary>
        internal const string MODULEPREFIX = "Vrh.ApplicationContainer";

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    StoptService();
                    // TODO: dispose managed state (managed objects).
                    if (_config != null)
                    {
                        _config.ConfigProcessorEvent -= _config_ConfigProcessorEvent;
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
