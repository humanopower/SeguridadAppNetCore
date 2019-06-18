using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Configuration;
using System.Globalization;
using DataAccessSecurity;
using EntityLibrary;
using SecurityLogicLibraryContracts;
using Microsoft.Extensions.Configuration;

namespace SecurityLogicLibrary
{
    public class RoleLogic : IRoleLogic
    {

		#region Atributes
		private IConfiguration _configuration;
		private string _applicationName;
		#endregion
		#region Constructor
		public RoleLogic(IConfiguration configuration)
		{
			_configuration = configuration;
			_applicationName = _configuration.GetConnectionString("SecurityConnectionString");
		}
		#endregion
		public Response AddNewRole(Role role, User registerUser, int tipo)
        {
            var response = new Response { Message = "Sin inicializar", Result = false };
            var userDa = new UserDA();
            var roleDa = new RoleDA(_configuration);
            try
            {


                #region AddUserDataValidation
                if (role.ApplicationId < 0)
                {
                    response.Message = "El IdAplicación no puede estar vacío";
                    return response;
                }

                if (string.IsNullOrEmpty(role.RoleName))
                {
                    response.Message = "El campo Nombre no puede estar vacío";
                    return response;
                }

                if (string.IsNullOrEmpty(role.RoleDescription))
                {
                    response.Message = "El campo descripción no puede estar vacío";
                    return response;
                }

                if (string.IsNullOrEmpty(role.RoleAuthorizationUserId))
                {
                    response.Message = "El campo autorizador no puede estar vacío";
                    return response;
                }

                if (string.IsNullOrEmpty(role.RoleAuthorizationOwner))
                {
                    response.Message = "El campo cargo no puede estar vacío";
                    return response;
                }

                if (registerUser == null)
                {
                    response.Message = "No se ha especificado el usuario con permisos para registrar";
                    return response;
                }

                var objUser = userDa.FindUser(role.RoleAuthorizationUserId);
                if (objUser == null)
                {
                    response.Message = "La clave del autorizador no es válida o no existe, favor de validar.";
                    return response;
                }
                #endregion



                //Valida Existencia Role
                var application = new UsersApplicationsRoles
                                      {
                                          ApplicationId = role.ApplicationId,
                                          UserId = registerUser.UserId
                                      };

                var lstrole = roleDa.GetRoleforApplication(application, tipo);
                var iRes = 0;

                if (lstrole.Any(roles => roles.RoleName.Equals(role.RoleName)))
                {
                    iRes = 1;
                }

                if (iRes <= 0)
                {
                    roleDa.AddRole(role, registerUser);
                    #region logRegister
                    var log = new Log
                    {
                        Application = new ApplicationPMX
                        {
                            ApplicationName = _applicationName
                        },
                        EventUser = registerUser,
                        EventTypeId = LogTypeEnum.Notification,
                        LogDescription = string.Format("Agregó el rol {0}-{1}  a la aplicación id {2}. Vigencia: {3} Autorizador rol: {4} Cargo autorizador:{5} Descripción: {6}",
                        role.RoleId, role.RoleName, role.ApplicationId, role.DeclineDate, role.RoleAuthorizationUserId, role.RoleAuthorizationOwner, role.RoleDescription)


                    };




                    #endregion
                    var loglogic = new LogLogic(_configuration);
                    loglogic.InsertLogEvent(log);
                    loglogic.Dispose();
                    response.Message = "Se registró correctamente el rol {0} para la Aplicación {1}";
                    response.Result = true;
                }
                else
                {
                    response.Message = "Ya esta asignado el rol a la aplicación, favor de verificar.";
                    response.Result = false;
                }
            }
            catch (Exception err)
            {
                if (err.Message.Substring(0, 35) == "Violation of PRIMARY KEY constraint")
                {
                    var log = new Log
                    {
                        EventUser = registerUser,
                        EventTypeId = LogTypeEnum.Notification,
                        LogDescription = string.Format("Se intentó agregar el rol {0} al esquema de seguridad, pero ya existe previamente", role.RoleId),
                        Application = new ApplicationPMX
                        {
                            ApplicationName = _applicationName
                        }
                    };
                    var loglogic = new LogLogic(_configuration);
                    loglogic.InsertLogEvent(log);
                    loglogic.Dispose();

                    response.Message = "El rol ha sido registrado previamente";
                    return response;
                }
                response.Message = string.Format("Ocurrio un error al intentar agregar el rol. {0} {1}", err.Message, DateTime.Now.ToString(CultureInfo.InvariantCulture));
                return response;
            }
            userDa.Dispose();
            roleDa.Dispose();

            return response;
        }

        public  Response UpdNewRole(Role role, User registerUser)
        {
            var response = new Response { Message = "Sin inicializar", Result = false };

            try
            {
                #region logRegister

                var log = new Log
                {
                    Application = new ApplicationPMX
                    {
                        ApplicationName = _applicationName
                    },
                    EventUser = registerUser,
                    EventTypeId = LogTypeEnum.Notification,
                    LogDescription = string.Format("Actualizó el rol {0}-{1}  a la aplicación id {2}. Vigencia: {3} Autorizador rol: {4} Cargo autorizador:{5} Descripción: {6}",
                    role.RoleId, role.RoleName, role.ApplicationId, role.DeclineDate, role.RoleAuthorizationUserId, role.RoleAuthorizationOwner, role.RoleDescription)

                };

                #endregion

                #region AddUserDataValidation
                if (role.ApplicationId < 0)
                {
                    response.Message = "El IdAplicación no puede estar vacío";
                    return response;
                }

                if (string.IsNullOrEmpty(role.RoleName))
                {
                    response.Message = "El campo Nombre no puede estar vacío";
                    return response;
                }

                if (string.IsNullOrEmpty(role.RoleDescription))
                {
                    response.Message = "El campo descripción no puede estar vacío";
                    return response;
                }

                if (registerUser == null)
                {
                    response.Message = "No se ha especificado el usuario con permisos para registrar";
                    return response;
                }

                #endregion

                var loglogic = new LogLogic(_configuration);
                var responseLog = loglogic.InsertLogEvent(log);
                loglogic.Dispose();
                if (!responseLog.Result)
                {
                    response.Message = string.Format("No se puede insertar en bitácorá el movimiento. {0}", responseLog.Message);
                    return response;
                }
                var roleDa = new RoleDA(_configuration);
                roleDa.UpdRole(role, registerUser);
                roleDa.Dispose();

                response.Message = "Se registró correctamente el rol para uso de Aplicaciones.";
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
                        LogDescription = string.Format("Se intentó agregar el rol {0} al esquema de seguridad, pero ya existe previamente", role.RoleId),
                        Application = new ApplicationPMX
                        {
                            ApplicationName = _applicationName
                        }
                    };
                    var loglogic = new LogLogic(_configuration);
                    loglogic.InsertLogEvent(log);
                    loglogic.Dispose();
                    response.Message = "El rol ha sido registrado previamente";
                    return response;
                }
                response.Message = string.Format("Ocurrio un error al intentar agregar el rol. {0} {1}", err.Message, DateTime.Now.ToString(CultureInfo.InvariantCulture));
                return response;
            }
            return response;
        }

        public  Response DelRole(Role role, User registerUser)
        {
            var response = new Response { Message = "Sin inicializar", Result = false };

            try
            {
                #region logRegister

                var log = new Log
                {
                    Application = new ApplicationPMX
                    {
                        ApplicationName = _applicationName
                    },
                    EventUser = registerUser,
                    EventTypeId = LogTypeEnum.Notification,
                    LogDescription = string.Format("Declinó el rol {0}-{1}  a la aplicación id: {2}. Vigente hasta: {3} Autorizador rol: {4} Cargo autorizador: {5} Descripción: {6}",
                     role.RoleId, role.RoleName, role.ApplicationId, role.DeclineDate, role.RoleAuthorizationUserId, role.RoleAuthorizationOwner, role.RoleDescription)

                };

                #endregion

                #region AddUserDataValidation
                if (role.RoleId < 0)
                {
                    response.Message = "La clave del rol no puede estar vacía.";
                    return response;
                }

                if (role.ApplicationId < 0)
                {
                    response.Message = "La clave de la aplicación no puede estar vacía.";
                    return response;
                }

                if (registerUser == null)
                {
                    response.Message = "No se ha especificado el usuario con permisos para registrar";
                    return response;
                }

                #endregion

                var loglogic = new LogLogic(_configuration);
                var responseLog = loglogic.InsertLogEvent(log);
                loglogic.Dispose();
                if (!responseLog.Result)
                {
                    response.Message = string.Format("No se puede insertar en bitácorá el movimiento. {0}", responseLog.Message);
                    return response;
                }


                var roleDa = new RoleDA(_configuration);
                roleDa.DelRole(role, registerUser);
                roleDa.Dispose();

                response.Message = "Se declino correctamente el rol para uso de Aplicaciones.";
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
                        LogDescription = string.Format("Se intentó eliminar el rol {0} al esquema de seguridad, pero ya existe previamente", role.RoleId),
                        Application = new ApplicationPMX
                        {
                            ApplicationName = _applicationName
                        }
                    };
                    var loglogic = new LogLogic(_configuration);
                    loglogic.InsertLogEvent(log);
                    loglogic.Dispose();
                    response.Message = "El rol ha sido eliminado previamente";
                    return response;
                }
                response.Message = string.Format("Ocurrio un error al intentar agregar el rol. {0} {1}", err.Message, DateTime.Now.ToString(CultureInfo.InvariantCulture));
                return response;
            }
            return response;
        }

        public  Role GetRole(Role role)
        {
            var roleDa = new RoleDA(_configuration);
            var roleresult = roleDa.GetRole(role.RoleId);
            roleDa.Dispose();
            return roleresult;
        }

        public  List<Role> GetRoles(string strValue)
        {
            var roleDa = new RoleDA(_configuration);
            var listrole = roleDa.GetRoles(strValue);
            roleDa.Dispose();
            return listrole;
        }

        public  List<Role> GetAllRole()
        {
            var roleDa = new RoleDA(_configuration);
            var listrole = roleDa.GetAllroles();
            roleDa.Dispose();
            return listrole;
        }

        public  List<Role> GetRoleList(ApplicationPMX application, User user)
        {
            var roleDa = new RoleDA(_configuration);
            var listrole = roleDa.GetRoleList(application, user);
            roleDa.Dispose();
            return listrole;
        }

        public  List<Operation> GetRoleOperation(int idRole)
        {
            var objRole = new Role { RoleId = idRole };
            var operationDa = new OperationDA(_configuration);
            var listOperations = operationDa.GetRoleOperations(objRole);
            operationDa.Dispose();
            return listOperations;
        }

        public  List<Role> GetRoleforApplications(UsersApplicationsRoles userApplicationRole, int tipo)
        {
            var roleda = new RoleDA(_configuration);
            var listRole = roleda.GetRoleforApplication(userApplicationRole, tipo);
            roleda.Dispose();
            return listRole;
        }

        public  Response AddRoleToApplication(ApplicationPMX application, Role role, User registerUser, int tipo)
        {
            var response = new Response { Message = "Sin inicializar", Result = false };
            var roleDa = new RoleDA(_configuration);
            try
            {
                //aplicationValidation
                var applicationLogic = new ApplicationLogic(_configuration);
                var applicationList = applicationLogic.GetApplicationList();
                applicationLogic.Dispose();

                var applicationFinded = applicationList.Find(app => app.ApplicationName == application.ApplicationName);
                if (applicationFinded == null)
                {
                    response.Message = string.Format("La aplicación {0} especificada no existe en el esquema de seguridad", application.ApplicationName);
                    return response;
                }


                //User validation
                var userLogic = new UserLogic(_configuration);
                var userFinded = userLogic.FindUser(registerUser.UserId);
                userLogic.Dispose();
                if (userFinded == null)
                {
                    response.Message = string.Format(
                        "El usuario {0} de registro no se encontró en el esquema de seguridad", registerUser.UserId);
                    return response;
                }

                //Role validation
                var userApplicationRole = new UsersApplicationsRoles()
                                              {
                                                  ApplicationId = applicationFinded.ApplicationId
                                              };

                var roleList = roleDa.GetRoleforApplication(userApplicationRole, tipo);
                var roleAlreadyFinded = roleList.Find(rolesearched => rolesearched.RoleName == role.RoleName);
                if (roleAlreadyFinded != null)
                {
                    response.Message = string.Format("El nombre {0} del rol no puede ser repetido.", role.RoleName);
                    return response;
                }
                //TODO:Validar nombre con caracteres distintos
                //Roleregister
                roleDa.AddRole(role, userFinded);

                //LogActivity
                var log = new Log
                {
                    Application = applicationFinded
                    ,
                    EventTypeId = LogTypeEnum.Notification
                    ,
                    EventUser = registerUser
                    ,
                    LogDescription =
                        string.Format("Se agregó el rol {0} a la aplicación {1}.", role.RoleName,
                                      applicationFinded.ApplicationName)

                };
                var loglogic = new LogLogic(_configuration);
                var resultLog = loglogic.InsertLogEvent(log);
                loglogic.Dispose();
                if (resultLog.Result)
                {
                    response.Message = string.Format("Se agregó el rol {0} a la aplicación {1}.", role.RoleName,
                                                     applicationFinded.ApplicationName);
                    response.Result = true;
                }
                else
                {
                    response.Message = string.Format("Se agregó el rol {0} a la aplicación {1}. Pero no se pudo registrar el movimiento en bitácora.", role.RoleName, applicationFinded.ApplicationName);
                    response.Result = true;
                }

            }
            catch (Exception err)
            {

                response.Message = string.Format("Ocurrio un error. {0}", err.Message);

            }
            roleDa.Dispose();
            return response;
        }

        public  DataTable GetRoleApplication(UsersApplicationsRoles userApplicationRole, int tipo)
        {
            var roleDa = new RoleDA(_configuration);
            var datatable = roleDa.GetRoleApplications(userApplicationRole, tipo);
            roleDa.Dispose();
            return datatable;
        }

        public  DataTable GetRolesApplications(string strValue)
        {
            var roleDa = new RoleDA(_configuration);
            var datatable = roleDa.GetRolesApplications(strValue);
            roleDa.Dispose();
            return datatable;
        }

        public  List<Role> GetRoleList(ApplicationPMX application)
        {
            var roleDa = new RoleDA(_configuration);
            var listrole = roleDa.GetRoleList(application);
            roleDa.Dispose();
            return listrole;
        }



        public  Response GetRoleCombinationNotAllowedByApplication(ApplicationPMX application, out DataTable roleCombinationsNotAllowed)
        {
            var response = new Response { Result = false, Message = "Not initialized" };
            roleCombinationsNotAllowed = new DataTable();
            var roleDa = new RoleDA(_configuration);
            try
            {
                roleCombinationsNotAllowed = roleDa.GetRoleCombinationsNotAllowed(application);
                response.Message =
                    string.Format("Se encontraron {0} combinaciones no permitidas para la aplicación {1}.",
                                  roleCombinationsNotAllowed.Rows.Count.ToString(), application.ApplicationName);
                response.Result = true;

            }
            catch (Exception e)
            {

                response.Message =
                    string.Format("Ocurrió un error al obtener las combinaciones de rol no autorizadas. {0}",
                                  e.Message);
                response.Result = false;
            }
            roleDa.Dispose();
            return response;
        }

        public  Response InsertRoleCombinationNotAllowed(ApplicationPMX application, Role roleAc, Role roleBc, User registerUser)
        {
            var response = new Response { Message = "Not Initialized", Result = false };
            var roleA = this.GetRole(roleAc);
            var roleB = this.GetRole(roleBc);


            if (roleA.RoleId == roleB.RoleId)
            {
                response.Message = "La combinación no puede realizarse con el mismo rol";
                response.Result = false;
                return response;
            }

            var resultExist = RoleNotAllowedCombinationExist(application, roleA, roleB);
            if (resultExist.Result)
            {
                response.Message = string.Format("No se puede insertar la combinación de rol.{0}", resultExist.Message);
                response.Result = false;
                return response;
            }


            var roleDa = new RoleDA(_configuration);
            try
            {
                roleDa.InsertRoleNotAllowedCombination(application, roleA, roleB, registerUser);
                response.Result = true;
                response.Message =
                            string.Format(
                                "Se insertó la combinación no permitida del rol {0} y {1} de la aplicación {2}.", roleA.RoleId.ToString() + " " + roleA.RoleName, roleB.RoleId.ToString() + " " + roleB.RoleName, application.ApplicationName);

                #region logRegister
                var log = new Log
                {
                    Application = new ApplicationPMX
                    {
                        ApplicationName = _applicationName
                    },
                    EventUser = registerUser,
                    EventTypeId = LogTypeEnum.Notification,
                    LogDescription = response.Message
                };

                #endregion
                var loglogic = new LogLogic(_configuration);
                loglogic.InsertLogEvent(log);
                loglogic.Dispose();

            }
            catch (Exception e)
            {

                response.Message =
                    string.Format(
                        "Ocurrio un error al insertar la combinación de roles. {0}",
                        e.Message);
                response.Result = false;
            }
            roleDa.Dispose();

            return response;
        }

        public  Response DeleteRoleCombinationNotAllowed(ApplicationPMX application, Role roleA, Role roleB)
        {
            //Se debe de regresar falso si no existe y se debe de interpretar en la llamada de la funcion
            var response = new Response { Message = "No existe la combinacion", Result = false };
            var roleDa = new RoleDA(_configuration);
            try
            {
                roleDa.DeleteRoleNotAllowedCombination(application, roleA, roleB);

            }
            catch (Exception e)
            {

                response.Message =
                    string.Format(
                        "Ocurrio un error al eliminar la combinacion de operaciones. {0}",
                        e.Message);
                response.Result = false;
            }
            response.Result = true;
            response.Message =
                        string.Format(
                            "Se eliminó la combinación no permitida de la operacion {0} y {1} de la aplicacion {2}.", roleA.RoleName, roleB.RoleName, application.ApplicationName);

            roleDa.Dispose();
            return response;

        }

        public  Response RoleNotAllowedCombinationExist(ApplicationPMX application, Role roleA, Role roleB)
        {
            //Se debe de regresar falso si no existe y se debe de interpretar en la llamada de la funcion
            var response = new Response { Message = "No existe la combinacion", Result = true };
            var roleDa = new RoleDA(_configuration);
            try
            {
                response.Result = roleDa.RoleNotAllowedCombinationExist(application, roleA, roleB);
                if (response.Result)
                {
                    response.Message =
                        string.Format(
                            "La combinación no permitida de rol se encuentra registrada.");

                }
                //else
                //{
                //    response.Message =
                //        string.Format(
                //            "La combinación no permitida de rol no existe registrada.");
                //}
                return response;
            }
            catch (Exception e)
            {

                response.Message =
                    string.Format(
                        "Ocurrio un error al verificar si la combinacion de operaciones no permititda existe. {0}",
                        e.Message);
                response.Result = true;
            }
            roleDa.Dispose();
            return response;
        }

        public Response RoleNotAllowedCombinationExistAndDate(ApplicationPMX application, Role roleA, Role roleB)
        {
            //Se debe de regresar falso si no existe y se debe de interpretar en la llamada de la funcion
            var response = new Response { Message = "No existe la combinacion", Result = true };
            var roleDa = new RoleDA(_configuration);
            try
            {
                response.Result = roleDa.RoleNotAllowedCombinationExistAndDate(application, roleA, roleB);
                if (response.Result)
                {
                    response.Message =
                        string.Format(
                            "La combinación no permitida de rol se encuentra registrada.");

                }

            }
            catch (Exception e)
            {

                response.Message =
                    string.Format(
                        "Ocurrio un error al verificar si la combinacion de operaciones no permititda existe. {0}",
                        e.Message);
                response.Result = true;
            }
            roleDa.Dispose();
            return response;
        }


        public  Response UpdateOperationCombinationNotAllowed(ApplicationPMX application, Role roleA, Role roleb, DateTime dtDeclineDate, User registerUser)
        {
            Response response = new Response() { Message = "Sin Inicializar", Result = false };
            var roleDa = new RoleDA(_configuration);
            try
            {
                roleDa.UpdateRoleNotAllowedCombination(application, roleA, roleb, dtDeclineDate, registerUser);
                response.Result = true;
                response.Message = "Combinacion no autorizada de rol actualizada.";
                #region logRegister
                var log = new Log
                {
                    Application = new ApplicationPMX
                    {
                        ApplicationName = _applicationName
                    },
                    EventUser = registerUser,
                    EventTypeId = LogTypeEnum.Notification,
                    LogDescription = string.Format("Se actualizó la fecha de vigencia de combinación no autorizada de roles. Roles {0} - {1} fecha de declinación {2}", roleA.RoleId.ToString() + " " + roleA.RoleName, roleb.RoleId.ToString() + " " + roleb.RoleName, dtDeclineDate.ToShortDateString())
                };

                #endregion

                var logLogic = new LogLogic(_configuration);
                logLogic.InsertLogEvent(log);
                logLogic.Dispose();
            }
            catch (Exception e)
            {

                response.Message = e.Message;
            }
            roleDa.Dispose();
            return response;
        }



        public void Dispose()
        {
          GC.SuppressFinalize(this);
        }
    }
}
