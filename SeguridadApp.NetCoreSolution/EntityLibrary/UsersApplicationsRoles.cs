using System;
using System.Runtime.Serialization;

namespace EntityLibrary
{
    [DataContract]
    public class UsersApplicationsRoles
    {
        [DataMember]
        public string UserId { get; set; }
        [DataMember]
        public string EmployeeNames { get; set; }
        [DataMember]
        public int ApplicationId { get; set; }
        [DataMember]
        public string ApplicationName { get; set; }
        [DataMember]
        public int RoleId { get; set; }
        [DataMember]
        public string RoleName { get; set; }
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
        [DataMember]
        public string ApplicationDescription { get; set; }
        [DataMember]
        public string Observations { get; set; }
        [DataMember]
        public string ValidityStartDate { get; set; }

    }
}
