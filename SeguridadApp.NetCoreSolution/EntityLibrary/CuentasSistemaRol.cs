using System;
using System.Runtime.Serialization;

namespace EntityLibrary
{
    [DataContract]
    public class CuentasSistemaRol
    {
        [DataMember]
        public string UserId { get; set; }
        [DataMember]
        public string EmployeeNumber { get; set; }
        [DataMember]
        public string EmployeeNames { get; set; }
        [DataMember]
        public DateTime DeclineDateU { get; set; }
        [DataMember]
        public DateTime DeclineDateR { get; set; }

    }
}
