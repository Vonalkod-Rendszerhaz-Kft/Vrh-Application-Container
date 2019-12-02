using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Vrh.ApplicationContainer.Control.Contract;
using Vrh.ApplicationContainer.Core;

namespace Plugin.Test
{
    public class TestPlugin : PluginAncestor
    {
        public TestPlugin()
        {
            EndLoad();
        }

        private static List<InstanceDefinition> GetInstances()
        {
            List<InstanceDefinition> instances = new List<InstanceDefinition>();
            instances.Add(
                    new InstanceDefinition()
                    {
                        Id = "TestInstance1",
                        Description = "TestInstance1",
                        Name = "TestInstance1",
                        InstanceConfig = "",
                        InstanceData = "1",
                        Type = new PluginDefinition()
                        {
                            Type = typeof(TestPlugin),
                            TypeName = typeof(TestPlugin).FullName,
                            AutoStart = false,
                            Singletone = false,
                            Version = FileVersionInfo.GetVersionInfo(typeof(TestPlugin).Assembly.Location).ProductVersion,
                            PluginConfig = "",
                        },
                    }
                );
            instances.Add(
                    new InstanceDefinition()
                    {
                        Id = "TestInstance2",
                        Description = "TestInstance2",
                        Name = "TestInstance2",
                        InstanceConfig = "",
                        InstanceData = "2",
                        Type = new PluginDefinition()
                        {
                            Type = typeof(TestPlugin),
                            TypeName = typeof(TestPlugin).FullName,
                            AutoStart = false,
                            Singletone = false,
                            Version = FileVersionInfo.GetVersionInfo(typeof(TestPlugin).Assembly.Location).ProductVersion,
                            PluginConfig = "",
                        },
                    }
                );
            return instances;
        }

        public static TestPlugin TestPluginFactory(InstanceDefinition instanceDefinition, string data)
        {
            var instance = new TestPlugin();
            instance._myData = instanceDefinition;
            instance._myData.InstanceData = data;
            return instance;
        }

        public override void Start()
        {
            try
            {
                Console.WriteLine("TEST START START");
                BeginStart();
                // Implement Plugin logic here
                Thread.Sleep(4000);
            }
            finally
            {
                Console.WriteLine("TEST START END");
                base.Start();
            }
        }

        public override void Stop()
        {
            try
            {
                Console.WriteLine("TEST STOP START");
                BeginStop();
                // Implement stop logic here
            }
            finally
            {
                Console.WriteLine("TEST STOP END");
                base.Stop();
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
                        Console.WriteLine("TEST DISPOSE START");
                        BeginDispose();
                        // TODO: dispose managed state (managed objects).
                    }
                    finally
                    {
                        Console.WriteLine("TEST DISPOSE END");
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
