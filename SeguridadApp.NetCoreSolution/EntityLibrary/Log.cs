using System;
using System.Runtime.Serialization;

namespace EntityLibrary
{
    [DataContract]
    public class Log
    {
        [DataMember]
        public int EventId { get; set; }
        [DataMember]
        public LogTypeEnum EventTypeId { get; set; }
        [DataMember]
        public DateTime EventDatetime { get; set; }
        [DataMember]
        public User EventUser { get; set; }
        [DataMember]
        public string EventIpAddress { get; set; }
        [DataMember]
        public ApplicationPMX Application { get; set; }
        [DataMember]
        public string LogDescription { get; set; }

    }
}
