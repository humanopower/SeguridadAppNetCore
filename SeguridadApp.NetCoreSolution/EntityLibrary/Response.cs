using System.Runtime.Serialization;

namespace EntityLibrary
{
    [DataContract]
    public class Response
    {
        public bool Result { get; set; }
        public string Message { get; set; }

    }
}
