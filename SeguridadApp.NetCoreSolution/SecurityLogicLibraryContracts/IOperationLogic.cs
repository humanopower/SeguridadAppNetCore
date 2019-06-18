using System;
using System.Collections.Generic;
using System.Data;
using EntityLibrary;

namespace SecurityLogicLibraryContracts
{
    public interface IOperationLogic : IDisposable
    {

        Response AddNewOperation(Operation operation, User registerUser);

        Response UpdOperation(Operation operation, User registerUser);

        Response DelOperation(Operation operation, User registerUser);

        List<Operation> GetOperationList(List<Role> listrole);

        List<Operation> GetOperationList(ApplicationPMX application);

        List<Operation> GetAllOperations();

        List<Operation> GetOperationByItems(string strValue);

        Operation GetOperationById(int operationId);

        List<Operation> GetOperationRole(Role role);

        Response AddOperationToApplication(ApplicationPMX application, Operation operation, User registerUser);

        Response UpdateRoleOperations(ApplicationPMX application, Role role, List<Operation> operationList,
            User registerUser, int tipoApp);

        Response AddOperationToRole(ApplicationPMX application, Operation operation, Role role, User registerUser);

        Response DeleteOperationToRole(ApplicationPMX application, Operation operation, Role role, User registerUser);

        //public static Response CanCombineOperationsInRole(ApplicationPMX application, Operation operationA, Operation operationB)
        //{
        //    var response = new Response() {Message = "Not Initialized", Result = false};
        //    var resultExist = OperationsNotAllowedCombinationExist(application, operationA, operationB);
        //    if (resultExist.Result)
        //    {
        //        response.Message = string.Format("No se puede combinar las operaciones.{0}", resultExist.Message);
        //        response.Result = false;
        //        return response;
        //    }


        //    response.Message = "Se puede combinar las operaciones no permitidas";
        //    response.Result = true;
        //    return response;
        //}


        Response GetOperationsCombinationNotAllowedByApplication(ApplicationPMX application,
            out DataTable operationsCombinationsNotAllowed);

        Response InsertOperationCombinationNotAllowed(ApplicationPMX application, Operation operationA,
            Operation operationB, DateTime declineDate, User registerUser);

        Response UpdateOperationCombinationNotAllowed(ApplicationPMX application, Operation operationA,
            Operation operationB, DateTime declineDate, User registerUser);

        Response OperationsNotAllowedCombinationExist(ApplicationPMX application, Operation operationA,
            Operation operationB);

        Response OperationsNotAllowedCombinationExistAndDate(ApplicationPMX application, Operation operationA,
            Operation operationB);

    }
}
