using System;

namespace EntityLibrary
{
	public class Log
	{
		public int EventId { get; set; }

		public LogTypeEnum EventTypeId { get; set; }

		public DateTime EventDatetime { get; set; }

		public User EventUser { get; set; }

		public string EventIpAddress { get; set; }

		public ApplicationPMX Application { get; set; }

		public string LogDescription { get; set; }
	}
}