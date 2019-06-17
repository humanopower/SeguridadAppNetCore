using System.Runtime.Serialization;

namespace EntityLibrary
{
    [DataContract]
    public class CuentasRolesAsignados
    {
        [DataMember]
        public int ApplicationId { get; set; }
        [DataMember]
        public string Cuenta { get; set; }
        [DataMember]
        public string Ficha { get; set; }
        [DataMember]
        public string Nombres { get; set; }
        [DataMember]
        public string Rol { get; set; }
    }
}
