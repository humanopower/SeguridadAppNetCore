using System;
using System.Runtime.Serialization;

namespace EntityLibrary
{
    [DataContract]
    public class RoleOperations
    {
        [DataMember]
        public int RoleId { get; set; }
        [DataMember]
        public int OperationId { get; set; }
        [DataMember]
        public DateTime CreationDateTime { get; set; }
        [DataMember]
        public string CreationUserId { get; set; }
        [DataMember]
        public DateTime ModificationDateTime { get; set; }
        [DataMember]
        public string ModificationUserId { get; set; }
        [DataMember]
        public string DeclineDate { get; set; }


    }
}
