using System;
using System.Runtime.Serialization;
namespace EntityLibrary
{
    [DataContract]
    public class User
    {
        [DataMember]
        public string UserId { get; set; }
        [DataMember]
        public string EmployeeNumber { get; set; }
        [DataMember]
        public string EmployeeNames { get; set; }
        [DataMember]

        public string EmployeeLastName { get; set; }
        [DataMember]
        public string EmployeeEmail { get; set; }
        [DataMember]
        public string Telephone { get; set; }
        [DataMember]
        public string MobileTelephone { get; set; }
        [DataMember]
        public string ValidityStartDate { get; set; }
        [DataMember]
        public string DeclineDate { get; set; }
        [DataMember]
        public string DeclineDateSIO { get; set; }
        [DataMember]
        public string Observations { get; set; }
        [DataMember]
        public string RegisterDate { get; set; }
        [DataMember]
        public DateTime LastUpdate { get; set; }
        [DataMember]
        public Guid SessionId { get; set; }
        [DataMember]
        public AuthenticationTypeEnum AuthenticationType { get; set; }
        [DataMember]
        public string OrganismCode { get; set; }
    }
}
