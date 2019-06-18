using System;
using System.Collections.Generic;

namespace EntityLibrary
{
	public class Role
	{
		public int RoleId { get; set; }

		public string RoleName { get; set; }

		public string RoleDescription { get; set; }

		public int ApplicationId { get; set; }

		public List<Operation> OperationsList { get; set; }

		public DateTime CreationDateTime { get; set; }

		public string CreationUserId { get; set; }

		public DateTime ModificationDateTime { get; set; }

		public string ModificationUserId { get; set; }

		public string DeclineDate { get; set; }

		public string RoleAuthorizationUserId { get; set; }

		public string RoleAuthorizationOwner { get; set; }
	}
}