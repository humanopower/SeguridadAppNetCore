using System;

namespace EntityLibrary
{
	public class Operation
	{
		public int OperationId { get; set; }

		public string OperationName { get; set; }

		public string OperationDescription { get; set; }

		public int ApplicationId { get; set; }

		public DateTime CreationDateTime { get; set; }

		public string CreationUserId { get; set; }

		public string ModificationDateTime { get; set; }

		public string ModificationUserId { get; set; }

		public string DeclineDate { get; set; }
	}
}