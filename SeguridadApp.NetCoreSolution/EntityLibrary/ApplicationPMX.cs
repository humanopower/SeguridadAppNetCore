using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EntityLibrary
{
    [DataContract]
    public class ApplicationPMX
    {
        [DataMember]
        public int ApplicationId { get; set; }
        [DataMember]
        public string ApplicationName { get; set; }
        [DataMember]
        public string ApplicationDescription { get; set; }
        [DataMember]
        public string ValidityStartDate { get; set; }
        [DataMember]
        public string DeclineDate { get; set; }

        [DataMember]
        public DateTime DeclineDateDF { get; set; }

        [DataMember]
        public string Observations { get; set; }
        [DataMember]
        public string TecnicalUserId { get; set; }
        [DataMember]
        public string TecnicalUserIdDos { get; set; }
        [DataMember]
        public string TecnicalUserIdTres { get; set; }
        [DataMember]
        public string TecnicalUserIdCuatro { get; set; }
        [DataMember]
        public string FunctionalUserId { get; set; }
        [DataMember]
        public DateTime CreationDatetime { get; set; }
        [DataMember]
        public string CreationUserId { get; set; }
        [DataMember]
        public DateTime ModificationDateTime { get; set; }
        [DataMember]
        public string ModificationUserId { get; set; }
        [DataMember]
        public string ApplicationPassword { get; set; }


        [DataMember]
        public List<Role> RolesList { get; set; }
    }
}
