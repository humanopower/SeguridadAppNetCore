using System;

namespace EntityLibrary
{
	public class UsersApplicationsRoles
	{
		public string UserId { get; set; }

		public string EmployeeNames { get; set; }

		public int ApplicationId { get; set; }

		public string ApplicationName { get; set; }

		public int RoleId { get; set; }

		public string RoleName { get; set; }

		public DateTime CreationDateTime { get; set; }

		public string CreationUserId { get; set; }

		public string ModificationDateTime { get; set; }

		public string ModificationUserId { get; set; }

		public string DeclineDate { get; set; }

		public string ApplicationDescription { get; set; }

		public string Observations { get; set; }

		public string ValidityStartDate { get; set; }
	}
}