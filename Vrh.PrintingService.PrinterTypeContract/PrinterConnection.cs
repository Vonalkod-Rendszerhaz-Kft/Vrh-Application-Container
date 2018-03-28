using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace Vrh.PrintingService.PrinterTypeContract
{
    public class PrinterConnection
    {
        private ConnectionTypes _connectionType;
        private string _connectionString;

        private TcpClient _client;
        private NetworkStream _writer;

        public PrinterConnection(ConnectionTypes connectionType, string connectionString)
        {
            _connectionType = connectionType;
            _connectionString = connectionString;
        }

        public ConnectionTypes ConnectionType
        {
            get { return _connectionType; }
        }

        public string ConnectionString
        {
            get { return _connectionString; }
        }

        public TcpClient Client
        {
            get { return _client; }
            set { _client = value; }
        }

        public NetworkStream Writer
        {
            get { return _writer; }
            set { _writer = value; }
        }
    }
}
