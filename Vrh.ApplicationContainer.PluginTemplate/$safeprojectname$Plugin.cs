using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace $safeprojectname$
{
    public class $safeprojectname$Plugin : PluginAncestor
    {
        /// <summary>
        /// Constructor
        /// </summary>
        private $safeprojectname$Plugin()
        {
            EndLoad();
        }

        /// <summary>
        /// Static Factory (Ha nincs megadva, akkor egy egy paraméteres konstruktort kell implementálni, amely konstruktor paraméterben fogja megkapni a )
        /// </summary>
        /// <param name="instanceDefinition">A példány definiciója</param>
        /// <param name="instanceData">A példánynak átadott adat(ok)</param>
        /// <returns></returns>
        public static $safeprojectname$Plugin ServiceStarterPluginFactory(InstanceDefinition instanceDefinition, Object instanceData)
        {
            var instance = new $safeprojectname$Plugin();
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

                base.Stop();
            }
            catch (Exception ex)
            {
                SetErrorState(ex);
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
