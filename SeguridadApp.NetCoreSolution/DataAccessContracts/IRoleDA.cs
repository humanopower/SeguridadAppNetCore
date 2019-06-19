using EntityLibrary;
using System;
using System.Collections.Generic;
using System.Data;

namespace DataAccessContracts
{
	public interface IRoleDA : IDisposable
	{
		void AddRole(Role role, User registerUser);

		void UpdRole(Role role, User registerUser);

		void DelRole(Role role, User registerUser);

		/// <summary>
		/// Metodo que obtiene la lista de roles asignados a un usuario previamente registrado.
		/// </summary>
		/// <param name="application">Objeto Application</param>
		/// <param name="user"> Objeto User</param>
		/// <returns>Lista de roles</returns>
		List<Role> GetRoleList(ApplicationPMX application, User user);

		Role GetRole(int roleId);

		List<Role> GetRoles(string strValue);

		DataTable GetRolesApplications(string strValue);

		List<Role> GetAllroles();

		/// <summary>
		/// Obtiene lista de roles por aplicacion
		/// </summary>
		/// <param name="userApplicationRole">Objeto tipo Aplicacion</param>
		/// /// <param name="iTipo">Tipo de Aplicacion</param>
		/// <returns>Lista de roles</returns>
		List<Role> GetRoleforApplication(UsersApplicationsRoles userApplicationRole, int iTipo);

		void UpdateRoleOperations(ApplicationPMX application, Role role, List<Operation> operations);

		DataTable GetRoleApplications(UsersApplicationsRoles userApplicationRole, int tipo);

		void CreateColumns();

		List<Role> GetRoleList(ApplicationPMX application);

		void InsertRoleNotAllowedCombination(ApplicationPMX application, Role roleA, Role roleB, User registerUser);

		void DeleteRoleNotAllowedCombination(ApplicationPMX application, Role roleA, Role roleB);

		bool RoleNotAllowedCombinationExist(ApplicationPMX application, Role roleA, Role roleB);

		bool RoleNotAllowedCombinationExistAndDate(ApplicationPMX application, Role roleA, Role roleB);

		DataTable GetRoleCombinationsNotAllowed(ApplicationPMX application);

		void UpdateRoleNotAllowedCombination(ApplicationPMX application, Role roleA, Role roleB, DateTime dtDeclineDate,
			 User registerUser);
	}
}