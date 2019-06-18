using System;
using System.Collections.Generic;

namespace EntityLibrary
{
	public class ApplicationPMX
	{
		public int ApplicationId { get; set; }

		public string ApplicationName { get; set; }

		public string ApplicationDescription { get; set; }

		public string ValidityStartDate { get; set; }

		public string DeclineDate { get; set; }

		public DateTime DeclineDateDF { get; set; }

		public string Observations { get; set; }

		public string TecnicalUserId { get; set; }

		public string TecnicalUserIdDos { get; set; }

		public string TecnicalUserIdTres { get; set; }

		public string TecnicalUserIdCuatro { get; set; }

		public string FunctionalUserId { get; set; }

		public DateTime CreationDatetime { get; set; }

		public string CreationUserId { get; set; }

		public DateTime ModificationDateTime { get; set; }

		public string ModificationUserId { get; set; }

		public string ApplicationPassword { get; set; }

		public List<Role> RolesList { get; set; }
	}
}