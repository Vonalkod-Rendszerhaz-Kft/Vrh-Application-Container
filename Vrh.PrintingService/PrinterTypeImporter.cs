using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using System.IO;
using Vrh.PrintingService.PrinterTypeContract;

namespace Vrh.PrintingService
{
    public class PrinterTypeImporter
    {
        [ImportMany]
        private IEnumerable<Lazy<IPrinterTypeComponent, IMetadata>> operations;

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

        public bool Connect(string printerType, string connectionType, string connectionString, out PrinterConnection printerConnection)
        {
            printerConnection = null;
            bool result = false;

            foreach (Lazy<IPrinterTypeComponent, IMetadata> com in operations)
            {
                Console.WriteLine(com.Value.Description);
                Console.WriteLine(com.Metadata.PrinterType);
                if (com.Metadata.PrinterType == printerType)
                {
                    result = com.Value.Connect(connectionType, connectionString, out printerConnection);
                }
            }

            return result;
        }

        public bool SendPrintCommand(string printerType, string labelData, PrinterConnection printerConnection, int maxTry, TimeSpan timeToWait)
        {
            bool result = false;

            foreach (Lazy<IPrinterTypeComponent, IMetadata> com in operations)
            {
                Console.WriteLine(com.Value.Description);
                Console.WriteLine(com.Metadata.PrinterType);
                if (com.Metadata.PrinterType == printerType)
                {
                    result = com.Value.SendPrintCommand(labelData, printerConnection, maxTry, timeToWait);
                }
            }

            return result;
        }

        public bool VerifyPrinterState(string printerType, PrinterConnection printerConnection, int maxTry, TimeSpan timeToWait)
        {
            bool result = false;

            foreach (Lazy<IPrinterTypeComponent, IMetadata> com in operations)
            {
                Console.WriteLine(com.Value.Description);
                Console.WriteLine(com.Metadata.PrinterType);
                if (com.Metadata.PrinterType == printerType)
                {
                    result = com.Value.VerifyPrinterState(printerConnection, maxTry, timeToWait);
                }
            }

            return result;
        }

        public void CloseConnection(string printerType, PrinterConnection printerConnection)
        {
            bool result = false;

            foreach (Lazy<IPrinterTypeComponent, IMetadata> com in operations)
            {
                Console.WriteLine(com.Value.Description);
                Console.WriteLine(com.Metadata.PrinterType);
                if (com.Metadata.PrinterType == printerType)
                {
                    com.Value.CloseConnection(printerConnection);
                }
            }
        }
    }
}
