using EntityLibrary;
using System;
using System.Collections.Generic;
using System.Data;

namespace DataAccessContracts
{
	public interface IOperationDA : IDisposable

	{
		void AddOperation(Operation operation, User registerUser);

		void UpdOperation(Operation operation, User registerUser);

		void DelOperation(Operation operation, User registerUser);

		List<Operation> GetAllOperations();

		/// <summary>
		/// Obtiene las operaciones de un grupo de roles
		/// </summary>
		/// <param name="roleList"></param>
		/// <returns></returns>
		List<Operation> GetOperationsList(List<Role> roleList);

		/// <summary>
		/// Obtiene lista de operaciones a partir de un rol especifico.
		/// </summary>
		/// <param name="role">Objeto tipo rol</param>
		/// <returns>Lista de operaciones</returns>
		List<Operation> GetRoleOperations(Role role);

		List<Operation> GetOperationsForItems(string strValue);

		Operation GetDataByIdOperation(int operationId);

		Operation GetDataByOperationName(Operation operation);

		List<Operation> GetOperationsList(ApplicationPMX application);

		void AddOperationToRole(Operation operation, Role role, User registerUser);

		void DeleteOperationToRole(Operation operation, Role role);

		bool OperationNotAllowedCombinationExist(ApplicationPMX application, Operation operationA,
			 Operation operationB);

		bool OperationNotAllowedCombinationExistAndDate(ApplicationPMX application, Operation operationA,
			 Operation operationB);

		void UpdateOperationNotAllowedCombination(ApplicationPMX application, Operation operationA, Operation operationB,
			 DateTime declineDate, User registerUser);

		void InsertOperationNotAllowedCombination(ApplicationPMX application, Operation operationA, Operation operationB,
			 DateTime declineDate, User registerUser);

		DataTable GetOperationsCombinationsNotAllowed(ApplicationPMX application);
	}
}