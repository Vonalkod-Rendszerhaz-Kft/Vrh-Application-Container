using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VRH.Common;
using Vrh.Logger;

namespace Vrh.ApplicationContainer
{
    /// <summary>
    /// Interface for Plugins 
    /// </summary>
    public interface IPlugin : IDisposable
    {
        /// <summary>
        /// Esemény a plugin állapotváltozás jelzésére
        /// </summary>
        event PluginStatusChangedEventHandler PluginStatusChanged;

        /// <summary>
        /// Plugin indíttása
        /// </summary>
        void Start();

        /// <summary>
        /// Plugin leállítása
        /// </summary>
        void Stop();

        /// <summary>
        /// Plugin példány státuszinformációi
        /// </summary>
        PluginStatus Status { get; }

        /// <summary>
        /// Plugin példány adatai 
        /// </summary>
        InstanceDefinition WhoAmI { get; set; }

        /// <summary>
        /// Pluginban keletkezett hibák gyűjtőstackje
        /// </summary>
        List<MessageStackEntry> Errors { get; }

        /// <summary>
        /// Plugin működésével kapcsolatos információk gyűjtőstackje
        /// </summary>
        List<MessageStackEntry> Infos { get; }
    }

    /// <summary>
    /// Delegate definició a plugin állapotváltozását jelző eseményhez 
    /// </summary>
    /// <param name="plugin"></param>
    public delegate void PluginStatusChangedEventHandler(Guid pluginId, PluginStateEnum condition);

    /// <summary>
    /// Ős plugin minden plugint ebből kell származtatni, ha fel akarjuk használni az előre elkészített implementációs részeket
    /// </summary>
    public class PluginAncestor : IPlugin
    {
        /// <summary>
        /// Constructor (protected, hogy ne lehessen közvetlenül példányosítani!!!)
        /// </summary>
        protected PluginAncestor()
        {
            BeginLoad();
        }

        /// <summary>
        /// Esemény, mely jelzi aplugin állapoitváltozását
        /// </summary>
        public event PluginStatusChangedEventHandler PluginStatusChanged;

        /// <summary>
        /// A plugin állapotváltozás esemény elsütése
        /// </summary>
        private void OnPluginStatusChanged()
        {
            lock (_locker)
            {
                PluginStatusChanged?.Invoke(_myData.InternalId, _myStatus);
            }
        }

        public PluginStatus Status
        {
            get
            {
                var status = new PluginStatus()
                {
                    State = _myStatus,
                    ErrorInfo = Logger.LogHelper.GetExceptionInfo(_myErrorStateInfo),
                };
                long size = 0;
                //object o = this;
                //using (Stream s = new MemoryStream())
                //{
                //    BinaryFormatter formatter = new BinaryFormatter();
                //    formatter.Serialize(s, o);
                //    size = s.Length;
                //}
                status.CurrentSizeInByte = size;
                return status;
            }
        }

        /// <summary>
        /// A plugin példány adatai
        /// </summary>
        public InstanceDefinition WhoAmI
        {
            get
            {
                lock (_locker)
                {
                    return _myData;
                }
            }
            set
            {
                lock (_locker)
                {
                    if (_myData == null)
                    {
                        _myData = value;
                    }
                }
            }
        }

        /// <summary>
        /// A pluginban keletkezet hibák gyűjteménye
        /// </summary>
        public List<MessageStackEntry> Errors
        {
            get
            {
                lock (_errors)
                {
                    return _errors.Items;
                }
            }
        }

        /// <summary>
        /// A plugin működési információinak gyűjteménye
        /// </summary>
        public List<MessageStackEntry> Infos
        {
            get
            {
                lock (_infos)
                {
                    return _infos.Items;
                }
            }
        }

        /// <summary>
        /// Plugin elindítása
        /// </summary>
        public virtual void Start()
        {
            try
            {
                BeginStart();
            }
            finally
            {
                EndStart();
            }
        }

        /// <summary>
        /// Plugin leállítása
        /// </summary>
        public virtual void Stop()
        {
            try
            {
                BeginStop();
            }
            finally
            {
                EndStop();
            }
        }

        /// <summary>
        /// Logol egy bejegyzést, tölti a message stackekekt is 
        /// </summary>
        /// <param name="message">Szöveges információ</param>
        /// <param name="data">adatok (kulcs-érték párként)</param>
        /// <param name="ex">Kivétel</param>
        /// <param name="level">Log szint</param>
        /// <param name="caller">Hivási hely (metódus)</param>
        /// <param name="line">Hivási hely (forrássor)</param>
        public void LogThis(string message, Dictionary<string, string> dataIn, Exception ex, Vrh.Logger.LogLevel level, Type type = null, [CallerMemberName]string caller = "", [CallerLineNumber]int line = 0)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            if (dataIn != null)
            {
                foreach (var item in dataIn)
                {
                    data.Add(item.Key, item.Value);
                }
            }
            VrhLogger.Log<string>(message, data, ex, level, type != null ? type : this.GetType(), caller, line);
            MessageStackEntry e = new MessageStackEntry()
            {
                Body = message,
                Data = data,
                TimeStamp = DateTime.UtcNow,
            };
            switch (level)
            {
                case Vrh.Logger.LogLevel.Information:
                case Vrh.Logger.LogLevel.Warning:
                    if (ex != null)
                    {
                        if (e.Data == null)
                        {
                            e.Data = new Dictionary<string, string>();
                        }
                        e.Data.Add("Exception", Vrh.Logger.LogHelper.GetExceptionInfo(ex));
                    }
                    e.Type = level == Vrh.Logger.LogLevel.Warning ? Level.Warning : Level.Info;
                    lock (_infos)
                    {
                        _infos.DropItem(e);
                    }
                    break;
                case Vrh.Logger.LogLevel.Error:
                case Vrh.Logger.LogLevel.Fatal:
                    if (e.Data == null)
                    {
                        e.Data = new Dictionary<string, string>();
                    }
                    e.Data.Add("Exception", Vrh.Logger.LogHelper.GetExceptionInfo(ex));
                    e.Type = level == Vrh.Logger.LogLevel.Fatal ? Level.FatalError : Level.Error;
                    lock (_errors)
                    {
                        _errors.DropItem(e);
                    }
                    break;
                default:
                    return;
            }
        }

        /// <summary>
        /// Ezt a metódust használjuk, ha hiba állapotba akarjuk tenni a plugint
        /// </summary>
        /// <param name="ex"></param>
        public void SetErrorState(Exception ex)
        {
            var data = new Dictionary<string, string>()
                        {
                            { "Id", _myData.Id },
                            { "Name", _myData.Name },
                            { "Type", _myData.Type.TypeName },
                        };
            LogThis(String.Format("Plagin status change from {0} to {1}", _myStatus, PluginStateEnum.Error), data, null, Logger.LogLevel.Information);
            lock (_locker)
            {
                _myStatus = PluginStateEnum.Error;
                _myErrorStateInfo = ex;
            }
            LogThis("Plugin go to Error state!", data, ex, Logger.LogLevel.Fatal);
            OnPluginStatusChanged();
        }

        /// <summary>
        /// A plugin példány státusz információja
        /// </summary>
        protected PluginStateEnum MyStatus
        {
            get
            {
                lock (_locker)
                {
                    return _myStatus;
                }
            }
            private set
            {
                lock (_locker)
                {
                    if (_myStatus != value)
                    {
                        if (value == PluginStateEnum.Unknown)
                        {
                            throw new Exception("Never set plugin to Unknown state!");
                        }
                        if (value == PluginStateEnum.Error)
                        {
                            throw new Exception("Use SetErrorState methode to set plugin to Error state!");
                        }
                        var data = new Dictionary<string, string>()
                        {
                            { "PluginType", this.GetType().FullName },
                            { "Version", this.GetType().Assembly.Version() },
                            { "InternalId", _myData?.InternalId.ToString() },
                            { "Plugin id", _myData?.Id },
                        };
                        LogThis(String.Format("Plagin status change from {0} to {1}", _myStatus, value), data, null, Logger.LogLevel.Information);
                        _myStatus = value;
                    }
                }
                OnPluginStatusChanged();
            }
        }

        /// <summary>
        /// Plugin töltésének a kezdete
        /// </summary>
        private void BeginLoad()
        {
            if (_myStatus != PluginStateEnum.Disposing && _myStatus != PluginStateEnum.Disposed)
            {
                MyStatus = PluginStateEnum.Loading;
            }
        }

        /// <summary>
        /// Ezt a metódust meg kell hívni amikor a plugin sikeresen befejezte az érdemi példányosítást
        /// </summary>
        protected void EndLoad()
        {
            if (_myStatus != PluginStateEnum.Disposing && _myStatus != PluginStateEnum.Disposed)
            {
                MyStatus = PluginStateEnum.Loaded;
            }
        }

        /// <summary>
        /// Ezt a metódust meg kell hívni amikor a plugin megkezdi az indítást (IPlugin.Start kezdete)
        /// </summary>
        protected void BeginStart()
        {
            if (_myStatus != PluginStateEnum.Disposing && _myStatus != PluginStateEnum.Disposed)
            {
                MyStatus = PluginStateEnum.Starting;
            }
        }

        /// <summary>
        /// Indítás IPlugin start vége
        /// </summary>
        private void EndStart()
        {
            if (_myStatus != PluginStateEnum.Disposing && _myStatus != PluginStateEnum.Disposed)
            {
                MyStatus = PluginStateEnum.Running;
            }
        }

        /// <summary>
        /// Ezt a metódust meg kell hívni amikor a plugin elkezdi a Leállást (Iplugin.Stop kezdete)
        /// </summary>
        protected void BeginStop()
        {
            if (_myStatus != PluginStateEnum.Disposing && _myStatus != PluginStateEnum.Disposed)
            {
                MyStatus = PluginStateEnum.Stopping;
            }
        }

        /// <summary>
        /// Leállás iPlugin.Stop vége
        /// </summary>
        private void EndStop()
        {
            if (_myStatus != PluginStateEnum.Disposing && _myStatus != PluginStateEnum.Disposed)
            {
                MyStatus = PluginStateEnum.Loaded;
            }
        }

        /// <summary>
        /// Ezt a metódust kell meghívni, mikor a plugin elkezdi a saját Dispose metódusának a meghívását
        /// </summary>
        protected void BeginDispose()
        {
            if (_myStatus == PluginStateEnum.Running)
            {
                Stop();
            }
            if (_myStatus != PluginStateEnum.Disposing && _myStatus != PluginStateEnum.Disposed)
            {
                MyStatus = PluginStateEnum.Disposing;
            }
        }

        /// <summary>
        /// Dispose vége
        /// </summary>
        private void EndDispose()
        {
            MyStatus = PluginStateEnum.Disposed;
        }

        /// <summary>
        /// plugin példány adatai
        /// </summary>
        protected InstanceDefinition _myData;

        /// <summary>
        /// plugin példány státusz információi
        /// </summary>
        private PluginStateEnum _myStatus = PluginStateEnum.Unknown;

        /// <summary>
        /// Erros státuszban ez itt tároljuk astátusszal kapcsolatos kivétel objektumot 
        /// </summary>
        protected Exception _myErrorStateInfo = null;

        /// <summary>
        /// Fix stack a pluginban fellépő hibák tárolására
        /// </summary>
        protected FixStack<MessageStackEntry> _errors = new FixStack<MessageStackEntry>(50);

        /// <summary>
        /// Fix stack a plugin működési információinak tárolására
        /// </summary>
        protected FixStack<MessageStackEntry> _infos = new FixStack<MessageStackEntry>(50);

        /// <summary>
        /// Instance level locker
        /// </summary>
        protected Object _locker = new Object();

        #region IDisposable Support
        protected bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Plugin felszabadítás
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    try
                    {
                        // TODO: dispose managed state (managed objects).
                        BeginDispose();
                    }
                    finally
                    {
                        EndDispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~PluginAncestor() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        /// <summary>
        /// Dispose hgívása kódból
        /// </summary>
        public virtual void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}


