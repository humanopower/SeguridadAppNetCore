using System.Collections.Generic;
using System.ServiceModel;
using EntityLibrary;


namespace SecurityServicesContracts
{
    [ServiceContract]
    public interface ISecurityServiceContract
    {
        [OperationContract]
        Response Authenticate(string domain, string userId, string password, string applicationName, out User userAuthenticated);
        [OperationContract]
        Response AuthenticateADOnly(string domain, string userId, string password, out User userAuthenticated);

        [OperationContract]
        Response Authorize(User userAuthenticaded, string applicationName, string applicationPassword, string operation);

        [OperationContract]
        Response InsertLog(Log log, string applicationPassword);

        [OperationContract]
        Response GetUserListByApplication(string applicationName, string applicationPassword, out List<User> userList);
        [OperationContract]
        Response GetUserListByRole(string applicationName, string applicationPassword, string roleName, out List<User> userList);

        [OperationContract]
        Response GetUserInformation(string employeeNumber, out User user);

        [OperationContract]
        Response GetUserInformationAndRoles(string applicationName, string applicationPassword, string userId, out User user, out List<Role> roleuser);

        [OperationContract]
        Response GetUserInformationAndOperations(User userAuthenticaded, string applicationName,
            string applicationPassword, out List<Operation> operationUserList);

        [OperationContract]
        Response GetUserInformationBySession(string guid, out User user);
    }
}
