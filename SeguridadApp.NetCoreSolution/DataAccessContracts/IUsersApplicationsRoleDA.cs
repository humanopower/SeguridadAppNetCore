using System;
using System.Collections.Generic;
using EntityLibrary;

namespace DataAccessContracts
{
    public interface IUsersApplicationsRoleDA : IDisposable
    {

        List<UsersApplicationsRoles> GetUsersApplicationsRoleList(User user, User loggedUser);

        void AddNewUsersApplicationsRoles(UsersApplicationsRoles usersApplicationsRoles, User registerUser);

        void DeleteUsersApplicationsRoles(UsersApplicationsRoles usersApplicationsRoles, User registerUser);

        List<UsersApplicationsRoles> GetApplicationRoleList(User user, User loggedUser);

        List<User> FindRoleUsers(Role role, ApplicationPMX application);

        List<UsersApplicationsRoles> GetApplicationUsersList(ApplicationPMX applicationPMX);
    }
}
