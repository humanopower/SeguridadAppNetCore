using System;

namespace EntityLibrary
{
	public class RoleOperations
	{
		public int RoleId { get; set; }

		public int OperationId { get; set; }

		public DateTime CreationDateTime { get; set; }

		public string CreationUserId { get; set; }

		public DateTime ModificationDateTime { get; set; }

		public string ModificationUserId { get; set; }

		public string DeclineDate { get; set; }
	}
}