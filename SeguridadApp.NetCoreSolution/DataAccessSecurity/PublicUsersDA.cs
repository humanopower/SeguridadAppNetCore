using DataAccessContracts;
using System;

namespace DataAccessSecurity
{
	public class PublicUsersDA : IPublicUsersDA

	{
		public string GetLastEncryptedPassword(string login)
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}
	}
}