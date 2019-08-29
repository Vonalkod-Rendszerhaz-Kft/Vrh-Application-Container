using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Timers;
using Vrh.Logger;
using Vrh.Redis.DataPoolHandler;

namespace Vrh.DataToRedisCore
{
    public class Process: IDisposable
    {
        public SqlConnection Connection { get; set; }
        public SqlCommand Command { get; set; }
        public SqlDataReader Reader { get; set; }
        public List<string> Columns { get; set; }
        public DataToRedisConfigXmlProcessor.UpdateProcess UpdateProcess { get; set; }
        public PoolHandler PoolHandler { get; set; }
        public DataToRedisConfigXmlProcessor.Pool Pool { get; set; }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    Columns = null;
                    UpdateProcess = null;
                    Pool = null;

                    if (Reader != null && !Reader.IsClosed)
                    {
                        Reader.Close();
                    }

                    Reader = null;

                    if (Command != null)
                    {
                        Command.Dispose();
                    }

                    if (Connection.State != ConnectionState.Closed)
                    {
                        Connection.Close();
                        Connection.Dispose();
                    }

                    //PoolHandler.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Process() {
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
