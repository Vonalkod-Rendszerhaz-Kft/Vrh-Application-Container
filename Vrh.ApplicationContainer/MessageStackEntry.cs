using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Vrh.ApplicationContainer
{
    [DataContract]
    public class MessageStackEntry
    {
        [DataMember]
        public DateTime TimeStamp { get; set; }

        [DataMember]
        public string Body { get; set; }

        [DataMember]
        public Dictionary<string, string> Data { get; set; }

        [DataMember]
        public Level Type { get; set; }
    }
}
