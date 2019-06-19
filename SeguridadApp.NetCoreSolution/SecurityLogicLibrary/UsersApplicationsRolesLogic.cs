using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using DataAccessSecurity;
using EntityLibrary;
using Microsoft.Extensions.Configuration;
using SecurityLogicLibraryContracts;

namespace SecurityLogicLibrary
{
    public class UsersApplicationsRolesLogic : IUsersApplicationsRolesLogic
    {
		#region Atributes
		private IConfiguration _configuration;
		private string _applicationName;
		#endregion
		#region Constructor
		public UsersApplicationsRolesLogic(IConfiguration configuration)
		{
			_configuration = configuration;
			_applicationName = _configuration.GetConnectionString("SecurityConnectionString");
		}
		#endregion

		#region Metodos Publicos
		public List<UsersApplicationsRoles> GetUsersApplicationsRoleList(User user, User loggedUser)
        {
            var usersApplicationsRoleDa = new UsersApplicationsRoleDA(_configuration);
            var usersApplicationsRoleList = usersApplicationsRoleDa.GetUsersApplicationsRoleList(user, loggedUser);
            usersApplicationsRoleDa.Dispose();
            return usersApplicationsRoleList;
        }

        public  List<UsersApplicationsRoles> GetApplicationsRoleList(User user, User loggedUser)
        {
            var usersApplicationsRoleDa = new UsersApplicationsRoleDA(_configuration);
            var usersApplicationsRoleList = usersApplicationsRoleDa.GetApplicationRoleList(user, loggedUser);
            usersApplicationsRoleDa.Dispose();
            return usersApplicationsRoleList;
        }

        public Response AddNewUsersApplicationsRoles(UsersApplicationsRoles usersApplicationsRoles, User registerUser)
        {
            var response = new Response { Message = "Sin inicializar", Result = false };
            try
            {
              

                #region AddUserDataValidation
                if (string.IsNullOrEmpty(usersApplicationsRoles.UserId))
                {
                    response.Message = "El usuario no puede estar vacío";
                    return response;
                }

                //Se valida que no exista previamente el rol asignado al usuario
                var usersApplicationsRoleLogic = new UsersApplicationsRolesLogic(_configuration);
                var lstprofileFull = usersApplicationsRoleLogic.GetApplicationsRoleList(new User(){UserId = usersApplicationsRoles.UserId},registerUser);
                usersApplicationsRoleLogic.Dispose();

                var roleFinded = lstprofileFull.Find(a => a.ApplicationName == usersApplicationsRoles.ApplicationName && a.RoleName == usersApplicationsRoles.RoleName);
                if(roleFinded != null)
                {
                    response.Message = "El rol ya ha sido asignado previamente al usuario.";
                    return response;
                }




                //Se valida que no exista la prohibición de combinacion de roles
                EntityLibrary.ApplicationPMX application = new ApplicationPMX() { ApplicationId = usersApplicationsRoles.ApplicationId };
                User usertoAdd = new User(){UserId = usersApplicationsRoles.UserId};
                var roleLogic = new RoleLogic(_configuration);
                List<EntityLibrary.Role> roles = roleLogic.GetRoleList(application, usertoAdd);
               
                
                Role roleB = new Role() { RoleId = usersApplicationsRoles.RoleId };

                foreach (var roleA in roles)
                {
                    var resultExist = roleLogic.RoleNotAllowedCombinationExistAndDate(application, roleA, roleB);
                    if (resultExist.Result)
                    {
                        response.Message =
                            "No se puede agregar el rol al usuario, ya que no está permitida en la combinación de roles";
                        return response;
                    }
                }
              roleLogic.Dispose();

                if (string.IsNullOrEmpty(usersApplicationsRoles.ApplicationId.ToString()))
                {
                    response.Message = "El campo IdAplicacion no puede estar vacío";
                    return response;
                }

                if (string.IsNullOrEmpty(usersApplicationsRoles.RoleId.ToString()))
                {
                    response.Message = "El campo IdRole no puede estar vacío";
                    return response;
                }

                var dateRole = Convert.ToDateTime(usersApplicationsRoles.DeclineDate);
                if ( Convert.ToDateTime(string.Format("{0:yyyy/MM/dd}", dateRole))< Convert.ToDateTime(string.Format("{0:yyyy/MM/dd}",DateTime.Now)))
                {
                    response.Message = "La fecha ingresada no puede ser menor a la fecha actual, favor de verificar.";
                    return response;
                }

                if (registerUser == null)
                {
                    response.Message = "No se ha especificado el usuario con permisos para registrar";
                    return response;
                }
                #endregion

                var usersApplicationsRoleDa = new UsersApplicationsRoleDA(_configuration);
                usersApplicationsRoleDa.AddNewUsersApplicationsRoles(usersApplicationsRoles, registerUser);
                  usersApplicationsRoleDa.Dispose();
                #region logRegister

                var log = new Log
                {
                    Application = new ApplicationPMX
                    {
                        ApplicationName = _applicationName
                    },
                    EventUser = registerUser,
                    EventTypeId = LogTypeEnum.Notification,
                    LogDescription = string.Format("Agregó el rol {0}-{1} de la aplicación {2}-{3} al usuario {4}  Vigente hasta: {5}.",
                                                    usersApplicationsRoles.RoleId, usersApplicationsRoles.RoleName, usersApplicationsRoles.ApplicationId, usersApplicationsRoles.ApplicationName, usersApplicationsRoles.UserId, usersApplicationsRoles.DeclineDate)
                };
                #endregion
                var loglogic = new LogLogic(_configuration);
                loglogic.InsertLogEvent(log);
                loglogic.Dispose();


                response.Message = "Se registró correctamente el rol {0} de la aplicación {1} al userId {2} - {3}.";
                response.Result = true;
            }
            catch (Exception err)
            {
                if (err.Message.Substring(0, 35) == "Violation of PRIMARY KEY constraint")
                {
                    var log = new Log
                    {
                        EventUser = registerUser,
                        EventTypeId = LogTypeEnum.Notification,
                        LogDescription = string.Format("Se intentó agregar al usuario {0} al esquema de seguridad, pero ya existe previamente", registerUser.UserId),
                        Application = new ApplicationPMX
                        {
                            ApplicationName =
                                _applicationName
                        }
                    };
                    var loglogic = new LogLogic(_configuration);
                    loglogic.InsertLogEvent(log);
                    loglogic.Dispose();
                    response.Message = "El usuario ya cuenta con el role seleccionado.";
                    return response;
                }
                response.Message = string.Format("Ocurrio un error al intentar agregar el usuario. {0} {1}", err.Message, DateTime.Now.ToString(CultureInfo.InvariantCulture));
                return response; 
            }
            return response;
        }

        public Response DelUsersApplicationsRoles(UsersApplicationsRoles usersApplicationsRoles, User registerUser)
        {
            var response = new Response { Message = "Sin inicializar", Result = false };
            try
            {
    

                #region AddUserDataValidation
                if (string.IsNullOrEmpty(usersApplicationsRoles.UserId))
                {
                    response.Message = "El usuario no puede estar vacío";
                    return response;
                }

                if (string.IsNullOrEmpty(usersApplicationsRoles.ApplicationId.ToString()))
                {
                    response.Message = "El campo IdAplicacion no puede estar vacío";
                    return response;
                }

                if (string.IsNullOrEmpty(usersApplicationsRoles.RoleId.ToString()))
                {
                    response.Message = "El campo IdRole no puede estar vacío";
                    return response;
                }

                if (string.IsNullOrEmpty(usersApplicationsRoles.DeclineDate.ToString()))
                {
                    response.Message = "El campo declinedate no puede estar vacío";
                    return response;
                }

                if (registerUser == null)
                {
                    response.Message = "No se ha especificado el usuario con permisos para registrar";
                    return response;
                }
                #endregion


                var usersApplicationsRoleDa = new  UsersApplicationsRoleDA(_configuration);
                usersApplicationsRoleDa.DeleteUsersApplicationsRoles(usersApplicationsRoles, registerUser);
                usersApplicationsRoleDa.Dispose();
                #region logRegister

                var log = new Log
                {
                    Application = new ApplicationPMX
                    {
                        ApplicationName = _applicationName
                    },
                    EventUser = registerUser,
                    EventTypeId = LogTypeEnum.Notification,
                    LogDescription = string.Format("Declinó el rol {0} de la aplicación {1} al usuario {2}. Fecha declinación {3} ",
                                                    usersApplicationsRoles.RoleName, usersApplicationsRoles.ApplicationName, usersApplicationsRoles.UserId, usersApplicationsRoles.DeclineDate)
                };
                #endregion
                var loglogic = new LogLogic(_configuration);
                loglogic.InsertLogEvent(log);
                loglogic.Dispose();
                

                response.Message = "Se declinó correctamente el usuario para uso de aplicaciones.";
                response.Result = true;
            }
            catch (Exception err)
            {


                if (err.Message.Substring(0, 35) == "Violation of PRIMARY KEY constraint")
                {
                    var log = new Log
                    {
                        EventUser = registerUser,
                        EventTypeId = LogTypeEnum.Notification,
                        LogDescription = string.Format("Se intentó declinar al usuario {0} al esquema de seguridad, pero ya existe previamente", usersApplicationsRoles.UserId),
                        Application = new ApplicationPMX
                        {
                            ApplicationName =
                                _applicationName
                        }
                    };
                    var loglogic = new LogLogic(_configuration);
                    loglogic.InsertLogEvent(log);
                    loglogic.Dispose();
                    response.Message = "El usuario ha sido declinado previamente";
                    return response;
                }
                response.Message = string.Format("Ocurrio un error al intentar agregar el usuario. {0} {1}", err.Message, DateTime.Now.ToString(CultureInfo.InvariantCulture));
                return response;
            }
            return response;
        }


        public List<User> FindRoleUsers(Role role, ApplicationPMX application)
        {
            var usersApplicationsRoleDa = new UsersApplicationsRoleDA(_configuration);
            var userList = usersApplicationsRoleDa.FindRoleUsers(role, application);
            usersApplicationsRoleDa.Dispose();
            return userList;
        }

        public List<User> GetApplicationUsersList(ApplicationPMX appFinded)
        {
            var usersApplicationsRoleDa = new UsersApplicationsRoleDA(_configuration);
            var lstAppUserRol = usersApplicationsRoleDa.GetApplicationUsersList(appFinded);
            usersApplicationsRoleDa.Dispose();
            var userWithDataList = new List<User>();
            var userDA = new UserDA(_configuration);
            foreach (var user in lstAppUserRol)
            {
                try
                {
                    User userData = userDA.FindUser(user.UserId);
                    if (userData != null)
                    {
                        userWithDataList.Add(userData);
                    }
                }
                catch (Exception e)
                {

                    string i = user.UserId;
                }
            }
            userDA.Dispose();
            return userWithDataList;
        }

        public void Dispose()
        {
           GC.SuppressFinalize(this);
        }
        #endregion

     



      
    }
}
