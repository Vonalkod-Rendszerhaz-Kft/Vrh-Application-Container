using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vrh.CameraService
{
    public class Camera
    {
        public Camera()
        {

        }

        public Camera(string name, string type, string protocol, bool enableInterventions, bool enableIOEXT, string connectionType, string connectionString)
        {
            Name = name;
            Type = type;
            Protocol = protocol;
            EnableInterventions = enableInterventions;
            EnableIOEXT = enableIOEXT;
            ConnectionType = connectionType;
            ConnectionString = connectionString;
        }

        public string Name { get; set; }
        public string Type { get; set; }
        public string Protocol { get; set; }
        public bool EnableInterventions { get; set; }
        public bool EnableIOEXT { get; set; }
        public string ConnectionType { get; set; }
        public string ConnectionString { get; set; }
    }
}
