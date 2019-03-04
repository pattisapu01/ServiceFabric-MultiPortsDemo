using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace WebGateway.Domain.Models
{
    [DataContract]
    public class PingResult
    {
        [DataMember]
        public long ReplicaId { get; set; }
        [DataMember]
        public string Message { get; set; }
    }
}
