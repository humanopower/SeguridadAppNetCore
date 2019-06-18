using System;
using System.Collections.Generic;
using EntityLibrary;

namespace SecurityLogicLibraryContracts
{
    public interface IApplicationLogic : IDisposable
    {
        /// <returns></returns>
        Response AddApplication(ApplicationPMX application, User registerUser, object[] args,List<string> cuentasUsuarios);

        Response UpdApplication(ApplicationPMX application, string strpwd, User registerUser,bool canAdminAppRolesAndOperations, bool canAdminUsers, object[] args);

        Response DelApplication(ApplicationPMX application, User registerUser);

        Response DelApplicationAdministration(ApplicationPMX application, User user);

        List<ApplicationPMX> GetApplicationList();

        List<ApplicationPMX> GetApplicationList(string strValue);

        ApplicationPMX GetApplication(int applicationId);

        List<ApplicationPMX> SearchApplications(string strValueApplication, ApplicationPMX application, User loggedUser);

        List<UsersApplicationsRoles> FindApplicationforUser(string strUser);

        object[] GetApplicationAdministration(ApplicationPMX application);

        void usuariosModificados(User registerUser, string mensaje);



        List<CuentasRolesAsignados> SearchCuentasRolesLogic(int idAplicacion);

        List<CuentasSistemaRol> SearchCuentasSistemaRolLogic(int cuentaUsuario);


    }
}
