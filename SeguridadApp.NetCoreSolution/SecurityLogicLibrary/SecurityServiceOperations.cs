using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.DirectoryServices;
using System.Security.Policy;
using System.Text;
using EntityLibrary;
using Microsoft.Extensions.Configuration;

namespace SecurityLogicLibrary
{
    public class SecurityServiceOperations : IDisposable
    {

		#region Atributes
		private IConfiguration _configuration;
		private string _applicationName;
		private string _LDapConnectionString;
		#endregion
		#region Constructor
		public SecurityServiceOperations(IConfiguration configuration)
		{
			_configuration = configuration;
			_applicationName = _configuration.GetConnectionString("SecurityConnectionString");
			_LDapConnectionString = _configuration.GetConnectionString("LDAPConnectionString");
		}
		#endregion
		/// <summary>
		/// Returns a true or false response against Active Directory and Application Security Service
		/// </summary>
		/// <param name="domain">string</param>
		/// <param name="userId">string</param>
		/// <param name="password">string</param>
		/// <param name="userAuthenticated">User</param>
		/// <param name="applicationName">applicationName</param>
		/// <param name="activeDirectoryAuthenticationRequired">activeDirectoryAuthenticationRequired</param>
		/// <returns>Response</returns>
		public Response Authenticate(string domain, string userId, string password, string applicationName, out User userAuthenticated)
        {   
            var response = new Response { Message = "Not initializated", Result = false };
            userAuthenticated = null;

            bool InActiveDirectory = false;

            //Security Service Validation
            try
            {
                var userLogic = new UserLogic(_configuration);
                userAuthenticated = userLogic.FindUser(userId);
                userLogic.Dispose();
                if (userAuthenticated == null)
                {
                    response.Message = "500 - La cuenta de usuario no existe en SeguridadApp.";
                    return response;
                }

            }
            catch (Exception securityException)
            {

                response.Message = string.Format("900 - Ocurrió un error al consultar el la cuenta de usuario en SeguridadApp: {0} ", securityException.Message);
                return response;
            }

            //ActiveDirectory Authentication            
            User AdUserFinded;
            this.GetUserInformation(userAuthenticated.EmployeeNumber, out AdUserFinded);
            
            if (AdUserFinded != null)
            {

                var pathLDap = _LDapConnectionString;
                string domainAndUsername;
                domainAndUsername = domain + @"\" + userAuthenticated.EmployeeNumber;
            
                var entry = new DirectoryEntry(pathLDap, domainAndUsername, password);
                try
                {
                    // Bind to the native AdsObject to force authentication.
                    var obj = entry.NativeObject;
                    var search = new DirectorySearcher(entry);
                    search.Filter = "(SAMAccountName=" + userAuthenticated.EmployeeNumber + ")";
                    search.PropertiesToLoad.Add("cn");
                    search.PropertiesToLoad.Add("mail");
                    search.PropertiesToLoad.Add("givenname");
                    search.PropertiesToLoad.Add("sn");
                    search.PropertiesToLoad.Add("samaccountname");
                    SearchResult result = search.FindOne();
                    if (null != result)
                    {
                        InActiveDirectory = true;
                    }
                    
                }
                catch (Exception ex)
                {
                    response.Message = string.Format
                        ("600 - No fue posible autenticar la cuenta de usuario en el Directorio Activo - {0}.  Intente nuevamente. Error: {1}",DateTime.Now.ToString(),
                         ex.Message);
                    return response;
                }

            }

            if (!InActiveDirectory)
            {
                response.Message = string.Format("600 - No fue posible autenticar la cuenta de usuario en el Directorio Activo. {0}",DateTime.Now.ToString());
                return response;
            }

            //Se valida la vigencia de fechas de la cuenta en SeguridadApp
           
            DateTime declineDate = new DateTime(Convert.ToInt32(userAuthenticated.DeclineDate.Substring(6, 4))
                , Convert.ToInt32(userAuthenticated.DeclineDate.Substring(3, 2))
                , Convert.ToInt32(userAuthenticated.DeclineDate.Substring(0, 2))); 
            DateTime declineDateSIO = new DateTime(Convert.ToInt32(userAuthenticated.DeclineDateSIO.Substring(6, 4))
                , Convert.ToInt32(userAuthenticated.DeclineDateSIO.Substring(3, 2))
                , Convert.ToInt32(userAuthenticated.DeclineDateSIO.Substring(0, 2)));
            if (declineDate <= DateTime.Now || declineDateSIO <= DateTime.Now)
            {
                response.Message = "501 - La cuenta de usuario no es vigente en SeguridadApp.";
                return response;
            }
            
            
            userAuthenticated.AuthenticationType = AuthenticationTypeEnum.SecurityServiceAndActiveDirectory;
            

            //Add session to the user 
            //1.-GetApplication Id
            var applicationLogic = new ApplicationLogic(_configuration);
            var applicationDbList = applicationLogic.GetApplicationList();
            applicationLogic.Dispose();
            var appFinded = applicationDbList.Find(app => app.ApplicationName == applicationName);
            if (appFinded == null)
            {
                response.Message = string.Format("700 - La aplicación {0} no existe en SeguridadApp. {1}", applicationName, DateTime.Now.ToString());
                return response;
            }

            //2.-AddSession to user
            var sessionLogic = new SessionLogic(_configuration);
            Response sessionResponse = sessionLogic.AddSession(userAuthenticated, appFinded);
            sessionLogic.Dispose();
            if (!sessionResponse.Result)
            {
                response.Message = string.Format("502 - No fue posible asignar una sesión a la cuenta de usuario en SeguridadApp");
                return response;
            }

            response.Result = true;
            response.Message = @"000 - La cuenta de Usuario se autenticó correctamente en SeguridadApp y Directorio Activo.";
            return response;

        }

        public Response AuthenticateADOnly(string domain, string userId, string password, out User userAuthenticated)
        {
            var response = new Response { Message = "Not initializated", Result = false };
            userAuthenticated = null;


            //ActiveDirectory Authentication            
            SearchResult ADSearchresult = null;


            var pathLDap = _LDapConnectionString;
            string domainAndUsername = domain + @"\" + userId;


            var entry = new DirectoryEntry(pathLDap, domainAndUsername, password);

            try
            {
                // Bind to the native AdsObject to force authentication.
                var obj = entry.NativeObject;
                //var search = new DirectorySearcher(entry, "(SAMAccountName=" + userId + ")");
                var search = new DirectorySearcher(entry);

                if (userAuthenticated == null)
                {
                    search.Filter = "(SAMAccountName=" + userId + ")";
                }
                else
                {
                    search.Filter = "(SAMAccountName=" + userAuthenticated.EmployeeNumber + ")";
                }
                search.PropertiesToLoad.Add("cn");
                search.PropertiesToLoad.Add("mail");
                search.PropertiesToLoad.Add("givenname");
                search.PropertiesToLoad.Add("sn");
                search.PropertiesToLoad.Add("samaccountname");
                ADSearchresult = search.FindOne();
            }
            catch (Exception ex)
            {
                response.Message = ("600 - No fue posible autenticar la cuenta de usuario en el Directorio Activo. Revise datos en SeguridadApp. Error: " + ex.Message);

            }

            if (null == ADSearchresult)
            {
                response.Result = false;
                response.Message = string.Format("No se pudo localizar el usuario en AD o su contraseña es erronea. {0:dd/MM/yyyy} {1:hh:mm:ss}", DateTime.Now, DateTime.Now);
                userAuthenticated = null;
                return response;
            }
            else
            {
                userAuthenticated = new User()
                {
                    EmployeeNames = GetProperty(ADSearchresult, "givenName"),
                    EmployeeLastName = GetProperty(ADSearchresult, "sn"),
                    EmployeeEmail = GetProperty(ADSearchresult, "mail"),
                    UserId = GetProperty(ADSearchresult, "mail"),
                    AuthenticationType = AuthenticationTypeEnum.ActiveDirectoryOnly
                };
            }

            if (string.IsNullOrEmpty(userAuthenticated.EmployeeNames))
            {
                response.Message = string.Format("No ha sido capturado el nombre del usuario en Directorio Activo ");
                response.Result = false;
                userAuthenticated = null;
                return response;
            }

            if (string.IsNullOrEmpty(userAuthenticated.EmployeeLastName))
            {
                response.Message = string.Format("No ha sido capturado el apellido del usuario en Directorio Activo. ");
                response.Result = false;
                userAuthenticated = null;
                return response;
            }

            if (string.IsNullOrEmpty(userAuthenticated.EmployeeEmail))
            {
                response.Message = string.Format("No ha sido capturado el correo electronico del usuario en Directorio Activo.");
                response.Result = false;
                userAuthenticated = null;
                return response;
            }


            response.Result = true;
            response.Message = string.Format("Se encontró el usuario en AD.");



            return response;
        }

        /// <summary>
        /// Returns a true or false response against Security schema
        /// </summary>
        /// <param name="userAuthenticaded"></param>
        /// <param name="applicationName"></param>
        /// <param name="applicationPassword"></param>
        ///  <param name="operation"></param>
        /// <returns></returns>
        public Response Authorize(User userAuthenticaded, string applicationName, string applicationPassword, string operation)
        {
            var response = new Response { Message = "Not initilizated", Result = false };
            try
            {
                //Validate data
                if (userAuthenticaded == null)
                {
                    response.Message = "502- Dato obligatorio: Cuenta de usuario.";
                    return response;
                }
                if (string.IsNullOrEmpty(applicationName))
                {
                    response.Message = "502- Dato obligatorio: Nombre de la aplicación.";
                    return response;
                }
                if (string.IsNullOrEmpty(applicationPassword))
                {
                    response.Message = "502- Dato obligatorio: Contraseña de la aplicación.";
                    return response;
                }
                if (string.IsNullOrEmpty(operation))
                {
                    response.Message = "502- Dato obligatorio: Nombre de la operación.";
                    return response;
                }

                //1.-GetApplication Id
                var applicationLogic = new ApplicationLogic(_configuration);
                var applicationDbList = applicationLogic.GetApplicationList();
                applicationLogic.Dispose();

                var appFinded = applicationDbList.Find(app => app.ApplicationName == applicationName);
                if (appFinded == null)
                {
                    response.Message = string.Format("700 - La aplicación {0} no existe en SeguridadApp. {1}", applicationName, DateTime.Now.ToString());
                    return response;
                }
                if (Convert.ToDateTime(appFinded.DeclineDate) < DateTime.Now)
                {
                    response.Message =
                        string.Format(
                            "607 - La aplicacion no se encuentra vigente. Si considera que la aplicación debe estar vigente, repórtelo a la extensión 811-49111.");
                    return response;
                }

                if (applicationPassword.Trim() != appFinded.ApplicationPassword.Trim())
                {
                    response.Message = string.Format("701 - La contraseña de aplicación es incorrecta. {0}", DateTime.Now.ToString());
                    return response;
                }

                //var applicationPasswordResult = Cryptographer.CompareHash("SecurityAlgorithim", applicationPassword, appFinded.ApplicationPassword);
                //if(!applicationPasswordResult)
                //{
                //    response.Message = string.Format("La contraseña de aplicación especificada no es válida. {0}",DateTime.Now.ToString());
                //    return response;
                //}


                //Using appFinded and user, we search for operations
                var userLogic = new UserLogic(_configuration);
                var userFinded = userLogic.FindUser(userAuthenticaded.UserId);
                userLogic.Dispose();

                if (userFinded == null)
                {
                    response.Message = string.Format("500 - La cuenta de usuario {0} no existe en SeguridadApp. {1}", userAuthenticaded.UserId, DateTime.Now.ToString());
                    return response;
                }
                var roleLogic = new RoleLogic(_configuration);
                List<Role> rolesFinded = roleLogic.GetRoleList(appFinded, userFinded);
                roleLogic.Dispose();
                if (rolesFinded.Count == 0)
                {
                    response.Message =
                        string.Format("503 - La cuenta de usuario {0} no tiene roles asignados. {1}", userAuthenticaded.UserId, DateTime.Now.ToString());
                    return response;
                }
                var operationLogic = new OperationLogic(_configuration);
                var operationsList = operationLogic.GetOperationList(rolesFinded);
                operationLogic.Dispose();
                if (operationsList.Count == 0)
                {
                    var sb = new StringBuilder();
                    sb.Append(string.Format("504 - La cuenta de usuario {0} no tiene operaciones/transacciones asignadas. {1}", userFinded.UserId, DateTime.Now.ToString()));
                    foreach (var role in rolesFinded)
                    {
                        sb.Append(string.Format("Operaciones buscadas para el rol id {0}, nombre rol {1}, d  ", role.RoleId, role.RoleName));
                    }
                    response.Message = sb.ToString();
                    return response;
                }

                //Search for the Operation specified.
                var operationFinded = operationsList.Find(operationsearched => operationsearched.OperationName == operation);
                if (operationFinded == null)
                {
                    response.Message = string.Format("La operación {0} solicitada, no está registrada en el esquema de seguridad o no está asignada al rol del usuario. {1}", operation, DateTime.Now.ToString());
                    return response;
                }

                //validate if User object has a valid sessionId  
                var sessionLogic = new SessionLogic(_configuration);
                var sessionValidation = sessionLogic.ValidateSession(userAuthenticaded, appFinded);
                sessionLogic.Dispose();
                if (!sessionValidation.Result)
                {
                    response.Message = string.Format("Sesión de usuario no válida.  {0}", sessionValidation.Message);
                    return response;
                }



                response.Result = true;
                response.Message = string.Format("Se validó correctamente la operacion {0} para la el usuario {1} y aplicacion {2}. {3}", operationFinded.OperationName, userFinded.UserId, appFinded.ApplicationName, DateTime.Now.ToString());

            }
            catch (Exception err)
            {

                response.Message = string.Format("Ocurrio un error al autorizar. {0} {1} ", err.Message, DateTime.Now.ToString());
                return response;
            }


            return response;
        }
        /// <summary>
        /// Returns a true or false response when trying to insert a log activitie.
        /// </summary>
        /// <param name="log">Log</param>
        /// <param name="applicationPassword">string</param>
        /// <returns></returns>
        public Response InsertLog(Log log, string applicationPassword)
        {
            var response = new Response { Message = "Sin inicilizar", Result = false };

            if (log.Application == null)
            {
                response.Message = "El objeto application no puede ser nulo para registro de bitacora.";
                return response;
            }

            if (string.IsNullOrEmpty(log.Application.ApplicationName))
            {
                response.Message = "El nombre de la aplicacion no puede ser vacío para registro de bitácora.";
                return response;
            }

            if (string.IsNullOrEmpty(log.LogDescription))
            {
                response.Message = "La descripcion del mensaje de bitácora no puede ser nulo o vacio";
                return response;
            }

            try
            {
                //1.-GetApplication Id
                var applicationLogic = new ApplicationLogic(_configuration);
                var applicationDbList = applicationLogic.GetApplicationList();
                applicationLogic.Dispose();
                var appFinded = applicationDbList.Find(app => app.ApplicationName == log.Application.ApplicationName);
                if (appFinded == null)
                {
                    response.Message = string.Format("La aplicación  {0} especificada no está registrada en esquema de seguridad. {1}", log.Application.ApplicationName, DateTime.Now.ToString());
                    return response;
                }

                /*Mantenimiento pendiente de criptografia de passwords de applicacion*/
                //var applicationPasswordResult = Cryptographer.CompareHash("SecurityAlgorithim", applicationPassword, appFinded.ApplicationPassword);
                //if (!applicationPasswordResult)
                //{
                //    response.Message = string.Format("La contraseña de aplicación especificada no es válida. {0}", DateTime.Now.ToString());
                //    return response;
                //}
            }
            catch (Exception err)
            {

                response.Message =
                    string.Format(
                        "Ocurrió un error al validar la aplicación para inserción de registro en bitácora. Err. {0}",
                        err.Message);
                return response;
            }

            try
            {
                var loglogic = new LogLogic(_configuration);
                var resultLog = loglogic.InsertLogEvent(log);
                loglogic.Dispose();
                response = resultLog;
            }
            catch (Exception err)
            {

                response.Message =
                    string.Format(
                        "Ocurrió un error al intentar insertar en bitácora. Err. {0}",
                        err.Message);
                return response;
            }
            return response;
        }

        /// <summary>
        /// Returns a user List that are contained in an application
        /// </summary>
        /// <param name="applicationName"></param>
        /// <param name="applicationPassword"></param>
        /// <param name="userList"></param>
        /// <returns></returns>
        public Response GetUserListByApplication(string applicationName, string applicationPassword, out List<User> userList)
        {
            var response = new Response { Message = "Not initilizated", Result = false };
            userList = new List<User>();
            try
            {
                //1.-GetApplication Id
                var applicationLogic = new ApplicationLogic(_configuration);
                var applicationDbList = applicationLogic.GetApplicationList();
                applicationLogic.Dispose();
                var appFinded = applicationDbList.Find(app => app.ApplicationName == applicationName);
                if (appFinded == null)
                {
                    response.Message = string.Format("La aplicación  {0} especificada no está registrada en esquema de seguridad. {1}", applicationName, DateTime.Now.ToString());
                    return response;
                }

                if (string.IsNullOrEmpty(applicationPassword))
                {
                    response.Message = string.Format("No ha sido proporcionado un password de aplicacion. {0}",
                                                     DateTime.Now);
                    return response;
                }

                //RS | 01/12/2015
                //DateTime declineDate = new DateTime(Convert.ToInt32(appFinded.DeclineDate.Substring(6, 4))
                //    ,Convert.ToInt32(appFinded.DeclineDate.Substring(0, 2))
                //    ,Convert.ToInt32(appFinded.DeclineDate.Substring(3, 2)) );

                //if (declineDate <= DateTime.Now)
                //{
                //    response.Message = string.Format("702 - La aplicación {0} no es vigente en SeguridadApp. {1}", applicationName, DateTime.Now.ToString());
                //    return response;
                //}

                if (appFinded.DeclineDateDF <= DateTime.Now)
                {
                    response.Message = string.Format("702 - La aplicación {0} no es vigente en SeguridadApp. {1}", applicationName, DateTime.Now.ToString());
                    return response;
                }


                if (applicationPassword.Trim() != appFinded.ApplicationPassword.Trim())
                {
                    response.Message = string.Format("701 - La contraseña de la aplicación {0} es incorrecta. {1}",applicationName, DateTime.Now.ToString());
                    return response;
                }

                //var applicationPasswordResult = Cryptographer.CompareHash("SecurityAlgorithim", applicationPassword, appFinded.ApplicationPassword);
                //if (!applicationPasswordResult)
                //{
                //    response.Message = string.Format("La contraseña de aplicación especificada no es válida. {0}", DateTime.Now.ToString());
                //    return response;
                //}

                var usersApplicationsRoleLogic = new UsersApplicationsRolesLogic(_configuration);
                List<User> listRoleUsers = usersApplicationsRoleLogic.GetApplicationUsersList(appFinded);
                usersApplicationsRoleLogic.Dispose();

                if (listRoleUsers.Count == 0)
                {
                    response.Message =
                        string.Format("No se encontró ningún usuario para la aplicación {0}", appFinded.ApplicationName);
                    return response;
                }

                userList = listRoleUsers;
                response.Message = string.Format("Se encontraron {0} usuarios de la aplicacion {1}",
                                                 userList.Count.ToString(), appFinded.ApplicationName);
                response.Result = true;
                return response;



            }
            catch (Exception exception)
            {

                response.Message = string.Format("Ocurrió un error al obtener la lista de usuarios por rol. Error {0}",
                                                 exception.Message);
                response.Result = false;
                return response;
            }

        }

        /// <summary>
        /// Returns a user List that are contained in the requested role.
        /// </summary>
        /// <param name="applicationName"></param>
        /// <param name="applicationPassword"></param>
        /// <param name="roleName"></param>
        /// <param name="userList"></param>
        /// <returns></returns>
        public Response GetUserListByRole(string applicationName, string applicationPassword, string roleName, out List<User> userList)
        {
            var response = new Response { Message = "Not initilizated", Result = false };
            userList = new List<User>();
            try
            {
                //1.-GetApplication Id
                var applicationLogic = new ApplicationLogic(_configuration);
                var applicationDbList = applicationLogic.GetApplicationList();
                applicationLogic.Dispose();
                var appFinded = applicationDbList.Find(app => app.ApplicationName == applicationName);
                if (appFinded == null)
                {
                    response.Message = string.Format("La aplicación  {0} especificada no está registrada en esquema de seguridad. {1}", applicationName, DateTime.Now.ToString());
                    return response;
                }


                if (applicationPassword.Trim() != appFinded.ApplicationPassword.Trim())
                {
                    response.Message = string.Format("La contraseña de aplicación especificada no es válida. {0}", DateTime.Now.ToString());
                    return response;
                }

                //var applicationPasswordResult = Cryptographer.CompareHash("SecurityAlgorithim", applicationPassword, appFinded.ApplicationPassword);
                //if (!applicationPasswordResult)
                //{
                //    response.Message = string.Format("La contraseña de aplicación especificada no es válida. {0}", DateTime.Now.ToString());
                //    return response;
                //}
                var roleLogic = new RoleLogic(_configuration);
                List<Role> applicationRoleList = roleLogic.GetRoleList(appFinded);
                roleLogic.Dispose();
                var roleFinded = applicationRoleList.Find(rolfinded => rolfinded.RoleName == roleName);
                if (roleFinded == null)
                {
                    response.Message = string.Format("El rol {0} no pudo ser encontrado en la aplicacion {1}", roleName,
                                                     appFinded.ApplicationName);
                    return response;
                }

                var usersApplicationsRoleLogic = new UsersApplicationsRolesLogic(_configuration);
                List<User> listRoleUsers = usersApplicationsRoleLogic.FindRoleUsers(roleFinded, appFinded);
                List<User> listaRoleUserVigentes= new List<User>();
                foreach (var roleUser in listRoleUsers)
                {
                    DateTime declineDate = Convert.ToDateTime(roleUser.DeclineDate);
                    DateTime declineDateSIO = Convert.ToDateTime(roleUser.DeclineDateSIO);
                    if (declineDate > DateTime.Now && declineDateSIO > DateTime.Now)
                    {
                        listaRoleUserVigentes.Add(roleUser);
                    }

                }

                usersApplicationsRoleLogic.Dispose();

                if (listaRoleUserVigentes.Count == 0)
                {
                    response.Message =
                        string.Format("No pudo ser encontrado ningun usuario para el rol {0} de la aplicación {1}",
                                      roleFinded.RoleName, appFinded.ApplicationName);
                    return response;
                }

                userList = listaRoleUserVigentes;
                response.Message = string.Format("Se encontraron {0} usuarios para el rol {1} de la aplicacion {2}",
                                                 userList.Count.ToString(), roleFinded.RoleName,
                                                 appFinded.ApplicationName);
                response.Result = true;
                return response;

                //UsersApplicationsRolesLogic.GetApplicationsRoleList()

            }
            catch (Exception exception)
            {

                response.Message = string.Format("Ocurrió un error al obtener la lista de usuarios por rol. Error {0}",
                                                 exception.Message);
                response.Result = false;
                return response;
            }

        }

        /// <summary>
        /// Returns user information in active directory
        /// </summary>
        /// <param name="employeeNumber">employeeNumber</param>
        /// <param name="user">user</param>
        /// <returns></returns>
        public Response GetUserInformation(string employeeNumber, out User user)
        {
            var response = new Response() { Message = "Not initializated.", Result = false };
            user = null;
            try
            {

                var pathLDap = _LDapConnectionString;

                var entry = new DirectoryEntry(pathLDap);
                // Bind to the native AdsObject to force authentication.
                var obj = entry.NativeObject;
                var search = new DirectorySearcher(entry);
                search.Filter = "(SAMAccountName=" + employeeNumber + ")";



                search.PropertiesToLoad.Add("cn");
                search.PropertiesToLoad.Add("mail");
                search.PropertiesToLoad.Add("givenname");
                search.PropertiesToLoad.Add("sn");
                search.PropertiesToLoad.Add("samaccountname");
                search.PropertiesToLoad.Add("EmployeeId");
                SearchResult result = search.FindOne();

                if (null == result)
                {
                    response.Message = string.Format("Usuario no pudo ser encontrado en directorio activo.");
                    response.Result = false;
                    user = null;
                    return response;
                }
                user = new User()
                {
                    EmployeeNames = GetProperty(result, "givenName"),
                    EmployeeLastName = GetProperty(result, "sn"),
                    EmployeeEmail = GetProperty(result, "mail"),
                    AuthenticationType = AuthenticationTypeEnum.ActiveDirectoryOnly
                };

                if (string.IsNullOrEmpty(user.EmployeeNames))
                {
                    response.Message = string.Format("No ha sido capturado el nombre del usuario en Directorio Activo ");
                    response.Result = false;
                    user = null;
                    return response;
                }
                if (string.IsNullOrEmpty(user.EmployeeLastName))
                {
                    response.Message = string.Format("No ha sido capturado el apellido del usuario en Directorio Activo. ");
                    response.Result = false;
                    user = null;
                    return response;
                }
                if (string.IsNullOrEmpty(user.EmployeeEmail))
                {
                    response.Message = string.Format("No ha sido capturado el correo electronico del usuario en Directorio Activo.");
                    response.Result = false;
                    user = null;
                    return response;
                }

                response.Message = string.Format("Se encontró el usuario {0} en directorio activo.", employeeNumber);
                response.Result = true;
              
            }
            catch (Exception exception)
            {

                response.Message =
                    string.Format("Ocurrio un error al consultar información de usuario en Directorio Activo. {0}", exception.Message);
                response.Result = false;
                user = null;
             
            }
            return response;
        }
        public Response GetUserInformationBySession(string guid, out User user)
        {
            var response = new Response() { Message = "Not initializated.", Result = false };
            user = null;
            try
            {

                //Se valida que la sesion no esté vacía

                if (string.IsNullOrEmpty(guid))
                {
                    response.Message = string.Format("No se ha proporcionado una sesión valida.");
                    response.Result = false;
                    user = null;
                    return response;
                }

                //Se busca la sesion activa en la bd.
                User userFinded = null;
                var sessionLogic = new SessionLogic(_configuration);
                Response userFindedResponse = sessionLogic.FindUserBySession(guid, out userFinded);
                sessionLogic.Dispose();

                if (!userFindedResponse.Result)
                {
                    response = userFindedResponse;

                    return response;
                }
                else
                {
                    user = userFinded;
                    response.Result = true;
                    response.Message = "Se encontró la sesion";
                }

              
            }
            catch (Exception exception)
            {

                response.Message =
                    string.Format("Ocurrio un error al consultar información de usuario en esquema de seguridad. {0}", exception.Message);
                response.Result = false;
                user = null;
            }
            return response;
        }

        public Response GetUserInformationAndRoles(string applicationName, string applicationPassword, string userId, out User user, out List<Role> roleUserList)
        {
            var response = new Response { Message = "Not initilizated", Result = false };
            user = new User();
            roleUserList = new List<Role>();
            try
            {
                //Validate data
                if (string.IsNullOrEmpty(userId))
                {
                    response.Message = "No se puede enviar el objeto user como nulo.";
                    return response;
                }
                if (string.IsNullOrEmpty(applicationName))
                {
                    response.Message = "No se puede enviar el nombre de la aplicación como nulo.";
                    return response;
                }
                if (string.IsNullOrEmpty(applicationPassword))
                {
                    response.Message = "No se puede enviar la contraseña de la aplicación como nulo.";
                    return response;
                }


                //1.-GetApplication Id
                var applicationLogic = new ApplicationLogic(_configuration);
                var applicationDbList = applicationLogic.GetApplicationList();
                applicationLogic.Dispose();
                var appFinded = applicationDbList.Find(app => app.ApplicationName == applicationName);
                if (appFinded == null)
                {
                    response.Message = string.Format("La aplicación  {0} especificada no está registrada en esquema de seguridad. {1}", applicationName, DateTime.Now.ToString());
                    return response;
                }


                if (applicationPassword.Trim() != appFinded.ApplicationPassword.Trim())
                {
                    response.Message = string.Format("La contraseña de aplicación especificada no es válida. {0}", DateTime.Now.ToString());
                    return response;
                }

                //var applicationPasswordResult = Cryptographer.CompareHash("SecurityAlgorithim", applicationPassword, appFinded.ApplicationPassword);
                //if (!applicationPasswordResult)
                //{
                //    response.Message = string.Format("La contraseña de aplicación especificada no es válida. {0}", DateTime.Now.ToString());
                //    return response;
                //}


                //Using appFinded and user, we search for operations
                var userLogic = new UserLogic(_configuration);
                var userFinded = userLogic.FindUser(userId);
                userLogic.Dispose();
                if (userFinded == null)
                {
                    response.Message = string.Format("El userId {0} no ha sido registrado en el esquema de seguridad. {1}", userId, DateTime.Now.ToString());
                    return response;
                }
                //Using user finded and applicationfinded, we look for the user roles.
                var roleLogic = new RoleLogic(_configuration);
                List<Role> rolesFinded = roleLogic.GetRoleList(appFinded, userFinded);
                roleLogic.Dispose();
                if (rolesFinded.Count == 0)
                {
                    response.Message =
                        string.Format("No ha sido asignado ningún rol al usuario {0} en el esquema de seguridad. {1}", userId, DateTime.Now.ToString());
                    return response;
                }

                user = userFinded;
                roleUserList = rolesFinded;

                StringBuilder sb = new StringBuilder();
                sb.Append("Se encontraron los roles : ");
                foreach (var role in roleUserList)
                {
                    sb.Append(string.Format(" {0}, ", role.RoleName));
                }
                sb.Append(string.Format("para el usuario {0} de la aplicación {1}", user.UserId, appFinded.ApplicationName));
                response.Result = true;
                response.Message = sb.ToString();
            }
            catch (Exception err)
            {

                response.Message = string.Format("Ocurrio un error al buscar información del usuario. {0} {1} ", err.Message, DateTime.Now.ToString());
                return response;
            }
            return response;
        }

        public Response GetUserInformationAndOperations(User userAuthenticaded, string applicationName, string applicationPassword, out List<Operation> operationUserList)
        {
            var response = new Response { Message = "Not initilizated", Result = false };
            operationUserList = new List<Operation>();
            try
            {
                //Validate data
                if (userAuthenticaded == null)
                {
                    response.Message = "No se puede enviar el objeto user como nulo.";
                    return response;
                }
                if (string.IsNullOrEmpty(applicationName))
                {
                    response.Message = "No se puede enviar el nombre de la aplicación como nulo.";
                    return response;
                }
                if (string.IsNullOrEmpty(applicationPassword))
                {
                    response.Message = "No se puede enviar la contraseña de la aplicación como nulo.";
                    return response;
                }


                //1.-GetApplication Id
                var applicationLogic = new ApplicationLogic(_configuration);
                var applicationDbList = applicationLogic.GetApplicationList();
                applicationLogic.Dispose();
                var appFinded = applicationDbList.Find(app => app.ApplicationName == applicationName);
                if (appFinded == null)
                {
                    response.Message = string.Format("La aplicación  {0} especificada no está registrada en esquema de seguridad. {1}", applicationName, DateTime.Now.ToString());
                    return response;
                }

                if (applicationPassword.Trim() != appFinded.ApplicationPassword.Trim())
                {
                    response.Message = string.Format("La contraseña de aplicación especificada no es válida. {0}", DateTime.Now.ToString());
                    return response;
                }

                //var applicationPasswordResult = Cryptographer.CompareHash("SecurityAlgorithim", applicationPassword, appFinded.ApplicationPassword);
                //if(!applicationPasswordResult)
                //{
                //    response.Message = string.Format("La contraseña de aplicación especificada no es válida. {0}",DateTime.Now.ToString());
                //    return response;
                //}


                //Using appFinded and user, we search for operations
                var userlogic = new UserLogic(_configuration);
                var userFinded = userlogic.FindUser(userAuthenticaded.UserId);
                userlogic.Dispose();
                if (userFinded == null)
                {
                    response.Message = string.Format("La cuenta {0} no existe en el esquema de seguridad. {1}", userAuthenticaded.UserId, DateTime.Now.ToString());
                    return response;
                }
                var roleLogic = new RoleLogic(_configuration);
                List<Role> rolesFinded = roleLogic.GetRoleList(appFinded, userFinded);
                roleLogic.Dispose();
                if (rolesFinded.Count == 0)
                {
                    response.Message =
                        string.Format("No ha sido asignado ningún rol al usuario {0} en el esquema de seguridad. {1}", userAuthenticaded.UserId, DateTime.Now.ToString());
                    return response;
                }



                //validate if User object has a valid sessionId  
                var sessionLogic = new SessionLogic(_configuration);
                var sessionValidation = sessionLogic.ValidateSession(userAuthenticaded, appFinded);
                sessionLogic.Dispose();
                if (!sessionValidation.Result)
                {
                    response.Message = string.Format("Sesión de usuario no válida.  {0}", sessionValidation.Message);
                    return response;
                }

                var operationLogic = new OperationLogic(_configuration);
                operationUserList = operationLogic.GetOperationList(rolesFinded);
                operationLogic.Dispose();
                response.Result = true;
                response.Message = string.Format("Se encontraron {0} operaciones para la el usuario {1} y aplicacion {2}. ", operationUserList.Count, userFinded.UserId, appFinded.ApplicationName, DateTime.Now.ToString());

            }
            catch (Exception err)
            {

                response.Message = string.Format("Ocurrio un error al autorizar. {0} {1} ", err.Message, DateTime.Now.ToString());
                return response;
            }


            return response;
        }
        private static string GetProperty(SearchResult searchResult, string propertyName)
        {
            if (searchResult.Properties.Contains(propertyName))
            {
                return searchResult.Properties[propertyName][0].ToString();
            }
            return string.Empty;

        }


        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
