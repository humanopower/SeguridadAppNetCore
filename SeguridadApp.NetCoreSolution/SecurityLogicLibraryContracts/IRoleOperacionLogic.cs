using System;
using System.Collections.Generic;
using EntityLibrary;

namespace SecurityLogicLibraryContracts
{
    public interface IRoleOperacionLogic : IDisposable
    {

        Response AddNewRoleOperation(RoleOperations roleOperations, User registerUser);

        Response DelRoleOperation(RoleOperations roleOperations, User registerUser);

        List<Operation> GetOperationRole(Role role);

        List<Role> GetOperationsList(Role roles);

       
    }
}
