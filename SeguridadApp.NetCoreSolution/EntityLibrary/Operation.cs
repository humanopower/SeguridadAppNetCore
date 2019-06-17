using System;
using System.Runtime.Serialization;

namespace EntityLibrary
{
    [DataContract]
    public class Operation
    {
        [DataMember]
        public int OperationId { get; set; }
        [DataMember]
        public string OperationName { get; set; }
        [DataMember]
        public string OperationDescription { get; set; }
        [DataMember]
        public int ApplicationId { get; set; }
        [DataMember]
        public DateTime CreationDateTime { get; set; }
        [DataMember]
        public string CreationUserId { get; set; }
        [DataMember]
        public string ModificationDateTime { get; set; }
        [DataMember]
        public string ModificationUserId { get; set; }
        [DataMember]
        public string DeclineDate { get; set; }
    }
}
