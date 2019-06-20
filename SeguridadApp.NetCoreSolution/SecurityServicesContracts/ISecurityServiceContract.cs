using System.Collections.Generic;

using EntityLibrary;


namespace SecurityServicesContracts
{

    public interface ISecurityServiceContract
    {
   
        Response Authenticate(string domain, string userId, string password, string applicationName, out User userAuthenticated);

        Response AuthenticateADOnly(string domain, string userId, string password, out User userAuthenticated);

        Response Authorize(User userAuthenticaded, string applicationName, string applicationPassword, string operation);


        Response InsertLog(Log log, string applicationPassword);


        Response GetUserListByApplication(string applicationName, string applicationPassword, out List<User> userList);

        Response GetUserListByRole(string applicationName, string applicationPassword, string roleName, out List<User> userList);

        Response GetUserInformation(string employeeNumber, out User user);

  
        Response GetUserInformationAndRoles(string applicationName, string applicationPassword, string userId, out User user, out List<Role> roleuser);

        Response GetUserInformationAndOperations(User userAuthenticaded, string applicationName,
            string applicationPassword, out List<Operation> operationUserList);
        Response GetUserInformationBySession(string guid, out User user);
    }
}
