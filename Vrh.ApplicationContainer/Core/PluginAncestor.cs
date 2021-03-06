using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vrh.ApplicationContainer.Control.Contract;
using Vrh.Logger;
using VRH.Common;

namespace Vrh.ApplicationContainer.Core
{
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

        /// <summary>
        /// Visszaadja  aplugin státuszát
        /// </summary>
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
        /// <param name="dataIn">adatok (kulcs-érték párként)</param>
        /// <param name="ex">Kivétel</param>
        /// <param name="level">Log szint</param>
        /// <param name="type"></param>
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
            VrhLogger.LogNested<string>(message, data, ex, level, type ?? this.GetType(), caller, line, stacklevel: 2);
            Dictionary<string, string> data2 = new Dictionary<string, string>();
            if (dataIn != null)
            {
                foreach (var item in dataIn)
                {
                    data2.Add(item.Key, item.Value);
                }
            }
            if (ex != null)
            {
                data2.Add("Exception", Vrh.Logger.LogHelper.GetExceptionInfo(ex));
            }
            MessageStackEntry e = new MessageStackEntry() { Body = message, Data = data2, TimeStamp = DateTime.UtcNow };
            switch (level)
            {
                case Vrh.Logger.LogLevel.Information:
                case Vrh.Logger.LogLevel.Warning:
                    e.Level = level == Vrh.Logger.LogLevel.Warning ? MessageStackEntryLevel.Warning : MessageStackEntryLevel.Info;
                    lock (_infos)
                    {
                        _infos.DropItem(e);
                    }
                    break;
                case Vrh.Logger.LogLevel.Error:
                case Vrh.Logger.LogLevel.Fatal:
                    e.Level = level == Vrh.Logger.LogLevel.Fatal ? MessageStackEntryLevel.FatalError : MessageStackEntryLevel.Error;
                    lock (_errors)
                    {
                        _errors.DropItem(e);
                    }
                    break;
                default:
                    break;
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
            LogThis($"Plugin status change from {_myStatus} to {PluginState.Error}", data, null, Logger.LogLevel.Information);
            lock (_locker)
            {
                _myStatus = PluginState.Error;
                _myErrorStateInfo = ex;
            }
            LogThis("Plugin go to Error state!", data, ex, Logger.LogLevel.Fatal);
            OnPluginStatusChanged();
        }

        /// <summary>
        /// A plugin példány státusz információja
        /// </summary>
        protected PluginState MyStatus
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
                        if (value == PluginState.Unknown)
                        {
                            throw new Exception("Never set plugin to Unknown state!");
                        }
                        if (value == PluginState.Error)
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
                        LogThis($"Plugin status change from {_myStatus} to {value}", data, null, Logger.LogLevel.Information);
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
            if (_myStatus != PluginState.Disposing && _myStatus != PluginState.Disposed)
            {
                MyStatus = PluginState.Loading;
            }
        }

        /// <summary>
        /// Ezt a metódust meg kell hívni amikor a plugin sikeresen befejezte az érdemi példányosítást
        /// </summary>
        protected void EndLoad()
        {
            if (_myStatus != PluginState.Disposing && _myStatus != PluginState.Disposed)
            {
                MyStatus = PluginState.Loaded;
            }
        }

        /// <summary>
        /// Ezt a metódust meg kell hívni amikor a plugin megkezdi az indítást (IPlugin.Start kezdete)
        /// </summary>
        protected void BeginStart()
        {
            if (_myStatus != PluginState.Disposing && _myStatus != PluginState.Disposed)
            {
                MyStatus = PluginState.Starting;
            }
        }

        /// <summary>
        /// Indítás IPlugin start vége
        /// </summary>
        private void EndStart()
        {
            if (_myStatus != PluginState.Disposing && _myStatus != PluginState.Disposed)
            {
                MyStatus = PluginState.Running;
            }
        }

        /// <summary>
        /// Ezt a metódust meg kell hívni amikor a plugin elkezdi a Leállást (Iplugin.Stop kezdete)
        /// </summary>
        protected void BeginStop()
        {
            if (_myStatus != PluginState.Disposing && _myStatus != PluginState.Disposed)
            {
                MyStatus = PluginState.Stopping;
            }
        }

        /// <summary>
        /// Leállás iPlugin.Stop vége
        /// </summary>
        private void EndStop()
        {
            if (_myStatus != PluginState.Disposing && _myStatus != PluginState.Disposed)
            {
                MyStatus = PluginState.Loaded;
            }
        }

        /// <summary>
        /// Ezt a metódust kell meghívni, mikor a plugin elkezdi a saját Dispose metódusának a meghívását
        /// </summary>
        protected void BeginDispose()
        {
            if (_myStatus == PluginState.Running)
            {
                Stop();
            }
            if (_myStatus != PluginState.Disposing && _myStatus != PluginState.Disposed)
            {
                MyStatus = PluginState.Disposing;
            }
        }

        /// <summary>
        /// Dispose vége
        /// </summary>
        private void EndDispose()
        {
            MyStatus = PluginState.Disposed;
        }

        /// <summary>
        /// plugin példány adatai
        /// </summary>
        protected InstanceDefinition _myData;

        /// <summary>
        /// plugin példány státusz információi
        /// </summary>
        private PluginState _myStatus = PluginState.Unknown;

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
        protected object _locker = new object();

        #region IDisposable Support
        /// <summary>
        /// To detect redundant calls
        /// </summary>
        protected bool disposedValue = false;

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
