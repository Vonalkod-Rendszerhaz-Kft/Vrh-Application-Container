using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace Vrh.CameraService.CameraTypeContract
{
    public class CameraConnection
    {
        private ProtocolTypes _protocolTypes;
        private IPAddress _ip;
        private int _listenPort;

        private TcpClient _client;
        private NetworkStream _writer;

        public CameraConnection(ProtocolTypes protocolType, IPAddress ip, int listenPort)
        {
            _protocolTypes = protocolType;
            _ip = ip;
            _listenPort = listenPort;
        }

        public ProtocolTypes ProtocolType
        {
            get { return _protocolTypes; }
        }

        public IPAddress IP
        {
            get { return _ip; }
        }

        public int ListenPort
        {
            get { return _listenPort; }
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
