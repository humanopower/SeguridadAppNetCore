using System;
using System.Collections.Generic;
using EntityLibrary;

namespace SecurityLogicLibraryContracts
{
    public interface IUsersApplicationsRolesLogic : IDisposable
    {


        List<UsersApplicationsRoles> GetUsersApplicationsRoleList(User user, User loggedUser);

        List<UsersApplicationsRoles> GetApplicationsRoleList(User user, User loggedUser);

        Response AddNewUsersApplicationsRoles(UsersApplicationsRoles usersApplicationsRoles, User registerUser);

        Response DelUsersApplicationsRoles(UsersApplicationsRoles usersApplicationsRoles, User registerUser);
        List<User> FindRoleUsers(Role role, ApplicationPMX application);

        List<User> GetApplicationUsersList(ApplicationPMX appFinded);
    }
}
