using System;

namespace DataAccessContracts
{
	public interface IResponsiveDA : IDisposable
	{
		void AddUserResponsive(string userId, int applicationId, int roleId, string userValidator);

		void AuthorizeUserResponsive(int responsiveId);
	}
}