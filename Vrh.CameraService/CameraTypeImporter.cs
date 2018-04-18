using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.IO;
using Vrh.CameraService.CameraTypeContract;

namespace Vrh.CameraService
{
    public class CameraTypeImporter
    {
        [ImportMany]
        private IEnumerable<Lazy<ICameraTypeComponent, IMetadata>> operations;

        public void DoImport()
        {
            //An aggregate catalog that combines multiple catalogs
            var catalog = new AggregateCatalog();

            //Add all the parts found in all assemblies in
            //the same directory as the executing program
            catalog.Catalogs.Add(
                    new DirectoryCatalog(
                        Path.GetDirectoryName(
                        Assembly.GetExecutingAssembly().Location
                        )
                )
            );        

            //Create the CompositionContainer with the parts in the catalog.
            CompositionContainer container = new CompositionContainer(catalog);

            //Fill the imports of this object
            var objectToSatisfy = this;
            container.ComposeParts(objectToSatisfy);
        }

        public int AvailableNumberOfOperation
        {
            get { return operations != null ? operations.Count() : 0; }
        }

        public void CreateListener(string cameraType,
                                   string protocol,
                                   string name,
                                   bool isAllowClientConnection,
                                   string connectionType,
                                   string connectionString,
                                   TimeSpan timeToWait,
                                   Action<string> cameraMessageAction)
        {
            foreach (Lazy<ICameraTypeComponent, IMetadata> com in operations)
            {
                Console.WriteLine(com.Value.Description);
                Console.WriteLine(com.Metadata.CameraType);
                if (com.Metadata.CameraType == cameraType)
                {
                    com.Value.CreateListener(name, protocol, isAllowClientConnection, connectionType, connectionString, timeToWait, cameraMessageAction);
                }
            }
        }

        public bool Connect(string cameraType, string protocol, string connectionType, string connectionString, out CameraConnection cameraConnection)
        {
            cameraConnection = null;
            bool result = false;

            foreach (Lazy<ICameraTypeComponent, IMetadata> com in operations)
            {
                Console.WriteLine(com.Value.Description);
                Console.WriteLine(com.Metadata.CameraType);
                if (com.Metadata.CameraType == cameraType)
                {
                    result = com.Value.Connect(protocol, connectionType, connectionString, out cameraConnection);
                }
            }

            return result;
        }

        public bool Read(string cameraType, CameraConnection cameraConnection, int maxTry, TimeSpan timeToWait)
        {
            bool result = false;

            foreach (Lazy<ICameraTypeComponent, IMetadata> com in operations)
            {
                Console.WriteLine(com.Value.Description);
                Console.WriteLine(com.Metadata.CameraType);
                if (com.Metadata.CameraType == cameraType)
                {
                    result = com.Value.Read(cameraConnection, maxTry, timeToWait);
                }
            }

            return result;
        }

        public bool GetIO(string cameraType, CameraConnection cameraConnection, int maxTry, TimeSpan timeToWait)
        {
            bool result = false;

            foreach (Lazy<ICameraTypeComponent, IMetadata> com in operations)
            {
                Console.WriteLine(com.Value.Description);
                Console.WriteLine(com.Metadata.CameraType);
                if (com.Metadata.CameraType == cameraType)
                {
                    result = com.Value.GetIO(cameraConnection, maxTry, timeToWait);
                }
            }

            return result;
        }

        public bool SetIO(int port, bool isOn, string cameraType, CameraConnection cameraConnection, int maxTry, TimeSpan timeToWait)
        {
            bool result = false;

            foreach (Lazy<ICameraTypeComponent, IMetadata> com in operations)
            {
                Console.WriteLine(com.Value.Description);
                Console.WriteLine(com.Metadata.CameraType);
                if (com.Metadata.CameraType == cameraType)
                {
                    result = com.Value.SetIO(port, isOn, cameraConnection, maxTry, timeToWait);
                }
            }

            return result;
        }

        public void CloseConnection(string cameraType, CameraConnection cameraConnection)
        {
            bool result = false;

            foreach (Lazy<ICameraTypeComponent, IMetadata> com in operations)
            {
                Console.WriteLine(com.Value.Description);
                Console.WriteLine(com.Metadata.CameraType);
                if (com.Metadata.CameraType == cameraType)
                {
                    com.Value.CloseConnection(cameraConnection);
                }
            }
        }
    }
}
