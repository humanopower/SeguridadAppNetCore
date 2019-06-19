using EntityLibrary;
using System;
using System.Collections.Generic;
using System.Data;

namespace DataAccessContracts
{
	public interface IApplicationDA : IDisposable
	{
		void AddApplication(ApplicationPMX application, User registerUser);

		void DelApplication(ApplicationPMX application, User registerUser);

		void UpdApplication(ApplicationPMX application, User registerUser);

		List<ApplicationPMX> GetApplicationList();

		List<ApplicationPMX> GetApplicationList(string strValue);

		DataSet SearchApplication(string strValue, ApplicationPMX application, User loggedUser);

		/// <summary>
		/// Metodo que obtiene las aplicaciones por Usuario
		/// </summary>
		/// <param name="strUser">Id del Usuario</param>
		/// <returns>Regresa un objeto de tipo DataSet</returns>
		DataSet FindApplicationforUser(string strUser);

		ApplicationPMX FindApplication(int idApplication);

		Response AddApplicationAdministration(ApplicationPMX application, User user,
			 bool canAdminAppRolesAndOperations, bool canAdminUsers, string userId);

		Response UpdApplicationAdministration(ApplicationPMX application,
			 bool canAdminAppRolesAndOperations, bool canAdminUsers, string strUser);

		Response DelApplicationAdministration(ApplicationPMX application, User user);

		object[] GetApplicationAdministration(ApplicationPMX application);

		DataSet SearchCuentasRolesData(int idAplicacion);

		DataSet SearchCuentasSistemaRol(int rolId);
	}
}