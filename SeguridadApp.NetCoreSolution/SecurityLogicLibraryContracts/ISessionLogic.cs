using System;
using EntityLibrary;

namespace SecurityLogicLibraryContracts
{
    public interface ISessionLogic : IDisposable
    {
        Response AddSession(User userAuthenticated, ApplicationPMX application);

        Response ValidateSession(User userAuthenticaded, ApplicationPMX appFinded);

        Response FindUserBySession(string sessionGuid, out User userFinded);
    }
}
