using EntityLibrary;
using System;

namespace DataAccessContracts
{
	public interface ISessionsDataAccess : IDisposable
	{
		User AddSession(User user, ApplicationPMX application);

		void FindSession(User user, ApplicationPMX application, out bool sessionFinded, out bool isSessionValid);

		void UpdateSessionEndTime(User user, ApplicationPMX application);

		string FindSessionUser(string sessionGuid);
	}
}