using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Vrh.CameraService.CameraTypeContract
{
    public interface ICameraTypeComponent
    {
        string Description { get; }

        void CreateListener(string protocol, 
                            string name,
                            bool IsAllowClientConnection,
                            string ConnectionType,
                            string ConnectionString,
                            TimeSpan TimeToWait,
                            Action<string> CameraMessageAction);

        bool Connect(string protocol, string ConnectionType, string ConnectionString, out CameraConnection cameraConnection);

        bool Read(CameraConnection cameraConnection, int maxTry, TimeSpan timeToWait);

        bool GetIO(CameraConnection cameraConnection, int maxTry, TimeSpan timeToWait);

        bool SetIO(int port, bool isOn, CameraConnection cameraConnection, int maxTry, TimeSpan timeToWait);

        void CloseConnection(CameraConnection cameraConnection);
    }

    public interface IMetadata
    {
        string CameraType { get; }
    }

}
