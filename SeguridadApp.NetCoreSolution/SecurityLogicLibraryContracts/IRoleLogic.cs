using System;
using System.Collections.Generic;
using System.Data;
using EntityLibrary;

namespace SecurityLogicLibraryContracts
{
    public interface IRoleLogic : IDisposable
    {

        Response AddNewRole(Role role, User registerUser, int tipo);

        Response UpdNewRole(Role role, User registerUser);

        Response DelRole(Role role, User registerUser);

        Role GetRole(Role role);

        List<Role> GetRoles(string strValue);

        List<Role> GetAllRole();

        List<Role> GetRoleList(ApplicationPMX application, User user);

        List<Operation> GetRoleOperation(int idRole);

        List<Role> GetRoleforApplications(UsersApplicationsRoles userApplicationRole, int tipo);

        Response AddRoleToApplication(ApplicationPMX application, Role role, User registerUser, int tipo);

        DataTable GetRoleApplication(UsersApplicationsRoles userApplicationRole, int tipo);

        DataTable GetRolesApplications(string strValue);

        List<Role> GetRoleList(ApplicationPMX application);



        Response GetRoleCombinationNotAllowedByApplication(ApplicationPMX application,
            out DataTable roleCombinationsNotAllowed);

        Response InsertRoleCombinationNotAllowed(ApplicationPMX application, Role roleAc, Role roleBc, User registerUser);

        Response DeleteRoleCombinationNotAllowed(ApplicationPMX application, Role roleA, Role roleB);

        Response RoleNotAllowedCombinationExist(ApplicationPMX application, Role roleA, Role roleB);

        Response RoleNotAllowedCombinationExistAndDate(ApplicationPMX application, Role roleA, Role roleB);


        Response UpdateOperationCombinationNotAllowed(ApplicationPMX application, Role roleA, Role roleb,
            DateTime dtDeclineDate, User registerUser);

    }
}
