using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vrh.PrintingService.PrinterTypeContract
{
    public interface IPrinterTypeComponent
    {
        string Description { get; }

        bool Connect(string connectionType, string connectionString, out PrinterConnection printerConnection);

        bool SendPrintCommand(string labelData, PrinterConnection printerConnection, int maxTry, TimeSpan timeToWait);

        bool VerifyPrinterState(PrinterConnection printerConnection, int maxTry, TimeSpan timeToWait);

        void CloseConnection(PrinterConnection printerConnection);
    }

    public interface IMetadata
    {
        string PrinterType { get; }
    }

}
