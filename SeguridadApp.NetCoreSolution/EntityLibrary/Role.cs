using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EntityLibrary
{
    [DataContract]
    public class Role
    {
                [DataMember]
        public int RoleId { get; set; }
                [DataMember]
        public string RoleName { get; set; }
                [DataMember]
        public string RoleDescription { get; set; }
                [DataMember]
        public int ApplicationId { get; set; }
                [DataMember]
        public List<Operation> OperationsList { get; set; }
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
                [DataMember]
        public string RoleAuthorizationUserId { get; set; }
                [DataMember]
        public string RoleAuthorizationOwner { get; set; }
    }
}
