using System;

namespace EntityLibrary
{
	public class User
	{
		public string UserId { get; set; }

		public string EmployeeNumber { get; set; }

		public string EmployeeNames { get; set; }

		public string EmployeeLastName { get; set; }

		public string EmployeeEmail { get; set; }

		public string Telephone { get; set; }

		public string MobileTelephone { get; set; }

		public string ValidityStartDate { get; set; }

		public string DeclineDate { get; set; }

		public string DeclineDateSIO { get; set; }

		public string Observations { get; set; }

		public string RegisterDate { get; set; }

		public DateTime LastUpdate { get; set; }

		public Guid SessionId { get; set; }

		public AuthenticationTypeEnum AuthenticationType { get; set; }

		public string OrganismCode { get; set; }
	}
}