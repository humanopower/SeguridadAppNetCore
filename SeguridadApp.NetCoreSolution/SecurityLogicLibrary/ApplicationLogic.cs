using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Data;
using System.Linq;
using EntityLibrary;
using DataAccessSecurity;
using SecurityLogicLibraryContracts;
using Microsoft.Extensions.Configuration;

namespace SecurityLogicLibrary
{
    public class ApplicationLogic : IApplicationLogic
    {
		#region Atributes
		private IConfiguration _configuration;
		private string _applicationName;
		#endregion
		#region Constructor
		public ApplicationLogic(IConfiguration configuration)
		{
			_configuration = configuration;
			_applicationName = _configuration.GetConnectionString("SecurityConnectionString");
		}
		#endregion

		#region Metodos Publicos
		/// <summary>
		/// Adds application object to application Database
		/// </summary>
		/// <param name="application">Values required ApplicationName,ApplicationDescription, FunctionalUserId, TecnicalUserId,ValidityStartDate</param>
		/// <param name="registerUser">Values required UserId</param>
		/// <returns></returns>
		public Response AddApplication(ApplicationPMX application, User registerUser, object [] args, List<string> cuentasUsuarios)
        {
            var response = new Response {Message = "Sin inicializar.", Result = false};
            var applicartionDa = new ApplicationDA(_configuration);
            try
            {
                if (application == null)
                {
                    response.Message = "El objeto application no puede ser nulo";
                    return response;
                }
                if (registerUser == null)
                {
                    response.Message = "El objeto registerUser no puede ser nulo";
                    return response;
                }
                if(string.IsNullOrEmpty(application.ApplicationName))
                {
                    response.Message = "Debe de proporcionar un nombre a la aplicación.";
                    return response;
                }

                if(string.IsNullOrEmpty(application.ApplicationDescription) || application.ApplicationDescription.Length < 1)
                {
                    response.Message = "El nombre de la aplicación no puede estar vacía.";
                    return response;
                }

                var applications = GetApplicationList();
                var appFinded = applications.Find(app => app.ApplicationName == application.ApplicationName);
                if(appFinded != null)
                {
                    response.Message = "El nombre de la aplicación ya existe registrada en SeguridadApp";
                    return response;
                }

                //Application paswordHashed
                if(string.IsNullOrEmpty(application.ApplicationPassword) || application.ApplicationPassword.Length < 8)
                {
                    response.Message = "Debe de especificar un password para la aplicación. Longitud mínima 8 caracteres.";
                    return response;
                }
                
                /*Mantenimiento pendiente de criptografia de passwords de applicacion*/
                //We use EnterpriseLibrary 5.0 to generate a hashed password and store in db.
                //application.ApplicationPassword = Cryptographer.CreateHash("SecurityAlgorithim", application.ApplicationPassword);
                /*Fin Mantenimieto*/



                if(Convert.ToDateTime(application.ValidityStartDate).Year != DateTime.Now.Year)
                {
                    response.Message = "El año de vigencia de la aplicación no puede ser distinto al año actual";
                    return response;
                }
                if(string.IsNullOrEmpty(application.Observations))
                {
                    application.Observations = " ";
                }

                if (Convert.ToDateTime(application.ValidityStartDate) > Convert.ToDateTime(application.DeclineDate))
                {
                    response.Message = "La fecha Vigencia final no puede ser menor a la fecha de inicio";
                    return response;
                }

                //Tecnical user finded
                if(string.IsNullOrEmpty(application.TecnicalUserId))
                {
                    response.Message = "La cuenta del usuario no ha sido registrada previamente.";
                    return response;
                }
                var userLogic = new UserLogic(_configuration);
                var userTecnicalFinded = userLogic.FindUser(application.TecnicalUserId);
                userLogic.Dispose();
                if(userTecnicalFinded==null)
                {
                  response.Message = "La cuenta de usuario especificado no ha sido registrado previamente.";
                  return response;
                }
                
                //Functional user finded
                if(string.IsNullOrEmpty(application.FunctionalUserId))
                {
                    response.Message = "Es necesario especificar una cuenta de usuario que exista previamente registrada.";
                    return response;
                }
               
                var userFunctionalFinded = userLogic.FindUser(application.FunctionalUserId);
                userLogic.Dispose();
                if(userFunctionalFinded==null)
                {
                  response.Message = "La cuenta del usuario especificado no ha sido registrado previamente.";
                  return response;
                }

                
                //Log user
                var userFinded = userLogic.FindUser(registerUser.UserId);
                
                if(userFinded == null)
                {
                    response.Message = "la cuenta de usuario para registrar la aplicación no es válido.";
                    return response;
                }
                application.CreationUserId = registerUser.UserId;
                application.ModificationUserId = registerUser.UserId;





                applicartionDa.AddApplication(application, registerUser);
                var log = new Log
                {
                    Application = new ApplicationPMX { ApplicationName = _applicationName },
                    EventUser = registerUser,
                    EventTypeId = LogTypeEnum.Notification,
                    LogDescription = string.Format("Agregó la aplicación {0}-{1} en SeguridadApp. Descripción: {2} Vigente: {3} Vigente hasta: {4} responsable funcional {5} , Observaciones {6} ", application.ApplicationId, application.ApplicationName, application.ApplicationDescription, application.ValidityStartDate, application.DeclineDate, application.FunctionalUserId, application.Observations)
                };

                var loglogic = new LogLogic(_configuration);
               loglogic.InsertLogEvent(log);
                loglogic.Dispose();

                foreach (object o in args)
                {
                    applicartionDa.AddApplicationAdministration(application, userFunctionalFinded, true, true, o.ToString());
                }

               
                if (cuentasUsuarios != null)
                {
                    foreach (string usuario in cuentasUsuarios)
                    {
                        string[] splitUsuario = usuario.Split(new Char[] { ' ' });


                        var logCuentasUsuarios = new Log
                        {
                            Application = new ApplicationPMX { ApplicationName = _applicationName },
                            EventUser = registerUser,
                            EventTypeId = LogTypeEnum.Notification,
                            LogDescription = string.Format("Agregó la cuenta " + splitUsuario[0] + " para ejercer sus permisos de SeguridadApp en la aplicación " + application.ApplicationId + "-" + application.ApplicationName)
                        };
                       
                       
                        loglogic.InsertLogEvent(logCuentasUsuarios);
                       
                    }
                }

                loglogic.Dispose();
                response.Message = string.Format("Se agregó correctamente la nueva aplicación al esquema de seguridad. ");
                response.Result = true;
            }
            catch (Exception err)
            {
               
                    var log = new Log
                    {
                        EventUser = registerUser,
                        EventTypeId = LogTypeEnum.Notification,
                        LogDescription = string.Format("Se intentó agregar la aplicación {0} en SeguridadApp, pero ocurrio un error. {1}", application.ApplicationName, err.Message),
                        Application = new ApplicationPMX
                        {
                            ApplicationName =
                                _applicationName
                        }
                    };
                    var loglogic = new LogLogic(_configuration);
                    var responseLog = loglogic.InsertLogEvent(log);
                    loglogic.Dispose();

                response.Message= string.Format("Ocurrio un error al intentar agregar una nueva aplicación. {0} {1}", responseLog.Message, DateTime.Now.ToString(CultureInfo.InvariantCulture));
                response.Result = false;
            }
            applicartionDa.Dispose();

            return response;
        }

        public  Response UpdApplication(ApplicationPMX application, string strpwd, User registerUser, bool canAdminAppRolesAndOperations, bool canAdminUsers, object[] args)
        {
            var response = new Response { Message = "Sin inicializar.", Result = false };

            var applicationDa = new ApplicationDA(_configuration);

            try
            {
                if (application == null)
                {
                    response.Message = "El objeto application no puede ser nulo";
                    return response;
                }
                if (registerUser == null)
                {
                    response.Message = "El objeto registerUser no puede ser nulo";
                    return response;
                }
                if (string.IsNullOrEmpty(application.ApplicationName))
                {
                    response.Message = "Debe de proporcionar un nombre a la aplicación.";
                    return response;
                }

                if (string.IsNullOrEmpty(application.ApplicationDescription) || application.ApplicationDescription.Length < 1)
                {
                    response.Message = "El nombre de la aplicación no puede estar vacía.";
                    return response;
                }
                
                //Application paswordHashed
                if (string.IsNullOrEmpty(application.ApplicationPassword) || application.ApplicationPassword.Length < 8)
                {
                    response.Message = "Debe de especificar un password para la aplicación. Longitud mínima 8 caracteres.";
                    return response;
                }
                
                /*Mantenimiento pendiente de criptografia de passwords de applicacion*/
                //We use EnterpriseLibrary 5.0 to generate a hashed password and store in db.
                //if (!strpwd.Trim().Equals(application.ApplicationPassword.Trim()))
                //{
                //    application.ApplicationPassword = Cryptographer.CreateHash("SecurityAlgorithim", application.ApplicationPassword);
                //}


                if (string.IsNullOrEmpty(application.Observations))
                {
                    application.Observations = " ";
                }

                //Tecnical user finded
                if (string.IsNullOrEmpty(application.TecnicalUserId))
                {
                    response.Message = "Es necesario especificar un usuario técnico registrado previamente.";
                    return response;
                }
                var userLogic = new UserLogic(_configuration);
                var userTecnicalFinded = userLogic.FindUser(application.TecnicalUserId);
                userLogic.Dispose();
                if (userTecnicalFinded == null)
                {
                    response.Message = "El usuario técnico especificado no ha sido registrado previamente.";
                    return response;
                }

                //Log user
                var userFinded = userLogic.FindUser(registerUser.UserId);

                if (userFinded == null)
                {
                    response.Message = "El usuario para registrar la aplicación no es válido.";
                    return response;
                }
                application.CreationUserId = registerUser.UserId;
                application.ModificationUserId = registerUser.UserId;

                 foreach (object o in args)
                 {

                     applicationDa.UpdApplication(application, registerUser);
                     applicationDa.UpdApplicationAdministration(application, canAdminAppRolesAndOperations,
                                                                canAdminUsers, o.ToString());
                     response.Message =
                         string.Format("Se Actualizó correctamente la nueva aplicación en SeguridadApp. ");
                     response.Result = true;
                 }
                var log = new Log
                                   {
                                       Application = new ApplicationPMX {ApplicationName = _applicationName},
                                       EventUser = registerUser,
                                       EventTypeId = LogTypeEnum.Notification,
                                       LogDescription = string.Format("Actualizó la aplicación {0}-{1} en SeguridadApp. Descripción: {2} Vigente {3} Vigente hasta {4} responsable funcional {5} , Observaciones {6} ", application.ApplicationId, application.ApplicationName, application.ApplicationDescription, application.ValidityStartDate, application.DeclineDate, application.FunctionalUserId, application.Observations)
                                   };
                var loglogic = new LogLogic(_configuration);
                loglogic.InsertLogEvent(log);
                loglogic.Dispose();
                 
            }
            catch (Exception err)
            {

                var log = new Log
                {
                    EventUser = registerUser,
                    EventTypeId = LogTypeEnum.Notification,
                    LogDescription = string.Format("Se intentó agregar la aplicacion {0} a SeguridadApp, pero ocurrio un error. {1}", application.ApplicationName, err.Message),
                    Application = new ApplicationPMX
                    {
                        ApplicationName =
                            _applicationName
                    }
                };
                var loglogic = new LogLogic(_configuration);
                var responseLog = loglogic.InsertLogEvent(log);
                loglogic.Dispose();

                response.Message = string.Format("Ocurrió un error al intentar agregar una nueva aplicación. {0} {1}", responseLog.Message, DateTime.Now.ToString(CultureInfo.InvariantCulture));
                response.Result = false;
            }
            applicationDa.Dispose();
            return response;
        }

        public  Response DelApplication(ApplicationPMX application, User registerUser)
        {
            var response = new Response { Message = "Sin inicializar.", Result = false };
            var applicationDa = new ApplicationDA(_configuration);
            try
            {
                if (application == null)
                {
                    response.Message = "El objeto application no puede ser nulo";
                    return response;
                }
                if (registerUser == null)
                {
                    response.Message = "El objeto registerUser no puede ser nulo";
                    return response;
                }
                
                if (string.IsNullOrEmpty(application.DeclineDate))
                {
                    response.Message = "La fecha fin de vigencia no puede estar vacía.";
                    return response;
                }
               
                //Log user
                var userLogic = new UserLogic(_configuration);
                var userFinded = userLogic.FindUser(registerUser.UserId);
                userLogic.Dispose();

                if (userFinded == null)
                {
                    response.Message = "Usuario no válido para declinar la aplicación.";
                    return response;
                }
                application.CreationUserId = registerUser.UserId;
                application.ModificationUserId = registerUser.UserId;

                var log = new Log
                {
                    Application = new ApplicationPMX { ApplicationName = _applicationName },
                    EventUser = registerUser,
                    EventTypeId = LogTypeEnum.Notification,
                    LogDescription = string.Format("Declinó la aplicacion {0}-{1} de SeguridadApp. Fecha declinación {2}  ", application.ApplicationId,application.ApplicationName, application.DeclineDate )
                };
                var loglogic = new LogLogic(_configuration);
               loglogic.InsertLogEvent(log);
                loglogic.Dispose();


                applicationDa.DelApplication(application, registerUser);
                response.Message = string.Format("Se declino correctamente la aplicación de SeguridadApp. ");
                response.Result = true;
            }
            catch (Exception err)
            {

                var log = new Log
                {
                    EventUser = registerUser,
                    EventTypeId = LogTypeEnum.Notification,
                    LogDescription = string.Format("Se intentó declinar la aplicacion {0} en SeguridadApp, pero ocurrio un error. {1}", application.ApplicationName, err.Message),
                    Application = new ApplicationPMX
                    {
                        ApplicationName =
                            _applicationName
                    }
                };
                var loglogic = new LogLogic(_configuration);
                var responseLog = loglogic.InsertLogEvent(log);
                loglogic.Dispose();

                response.Message = string.Format("Ocurrio un error al intentar declinar una aplicacion. {0} {1}", responseLog.Message, DateTime.Now.ToString(CultureInfo.InvariantCulture));
                response.Result = false;
            }
            applicationDa.Dispose();
            return response;
        }

        public  Response DelApplicationAdministration(ApplicationPMX application,User user)
        {
            var response = new Response { Message = "Sin inicializar.", Result = false };
            var applicationDa = new ApplicationDA(_configuration);
            try
            {
                if (application == null)
                {
                    response.Message = "El objeto application no puede ser nulo";
                    return response;
                }
                if (user == null)
                {
                    response.Message = "El objeto user no puede ser nulo";
                    return response;
                }
                
                var log = new Log
                {
                    Application = new ApplicationPMX { ApplicationName = _applicationName },
                    EventUser = user,
                    EventTypeId = LogTypeEnum.Notification,
                    LogDescription = string.Format("Removió la cuenta {0} de la configuración que permite ejercer sus permisos de SeguridadApp en la aplicación {1}.", user.UserId, application.ApplicationName)
                };
                var loglogic = new LogLogic(_configuration);
                loglogic.InsertLogEvent(log);
                loglogic.Dispose();


                applicationDa.DelApplicationAdministration(application, user);
                response.Message =
                    string.Format(
                        "Se ha eliminado la cuenta del usuario {0} que permite ejercer sus permisos de SeguridadApp en la aplicación {1}.",
                        user.UserId, application.ApplicationName);
                response.Result = true;
            }
            catch (Exception err)
            {

                var log = new Log
                {
                    EventUser = user,
                    EventTypeId = LogTypeEnum.Notification,
                    LogDescription = string.Format("Se intentó eliminar la cuenta del usuario {0} para la aplicacion {1} , pero ocurrio un error. {2}", user.UserId, application.ApplicationName, err.Message),
                    Application = new ApplicationPMX
                    {
                        ApplicationName =
                            _applicationName
                    }
                };
                var loglogic = new LogLogic(_configuration);
                var responseLog = loglogic.InsertLogEvent(log);
                loglogic.Dispose();

                response.Message = string.Format("Ocurrio un error al intentar eliminar la cuenta del usuario {0} para la aplicacion. {1} {2}",user.UserId ,responseLog.Message, DateTime.Now.ToString(CultureInfo.InvariantCulture));
                response.Result = false;
            }
            applicationDa.Dispose();
            return response;
        }

        public  List<ApplicationPMX> GetApplicationList()
        {
            List<ApplicationPMX> appList;
            var applicationDa = new ApplicationDA(_configuration);
            try
            {
                appList = applicationDa.GetApplicationList();
            }
            catch (Exception)
            {
                
                throw;
            }
            applicationDa.Dispose();
            return appList;
        }

        public  List<ApplicationPMX> GetApplicationList(string strValue)
        {
            var applicationDa = new ApplicationDA(_configuration);
            var listApplication = applicationDa.GetApplicationList(strValue);
            applicationDa.Dispose();
            return listApplication;
        }

        public  ApplicationPMX GetApplication(int applicationId)
        {
            var applicationDa = new ApplicationDA(_configuration);
            var application = applicationDa.FindApplication(applicationId);
            applicationDa.Dispose();
            return application;
        }

        public List<ApplicationPMX> SearchApplications(string strValueApplication, ApplicationPMX application, User loggedUser)
        {
            var daApplication = new ApplicationDA(_configuration);
            var lstApplication = new List<ApplicationPMX>();

            var ds = daApplication.SearchApplication(strValueApplication, application,loggedUser);

            if (ds.Tables[0].Rows.Count>0)
            {
                lstApplication.AddRange(from DataRow item in ds.Tables[0].Rows
                                        select new ApplicationPMX
                                                   {
                                                       ApplicationId = Convert.ToInt32(item[0]), 
                                                       ApplicationName = item[1].ToString(), 
                                                       ApplicationDescription = item[2].ToString(), 
                                                       ValidityStartDate = string.IsNullOrEmpty(item[3].ToString()) ? string.Empty : string.Format("{0:dd/MM/yyyy}", item[3]), 
                                                       DeclineDate = string.IsNullOrEmpty(item[4].ToString()) ? string.Empty : string.Format("{0:dd/MM/yyyy}", item[4]), 
                                                       Observations = item[5].ToString()
                                                   });
            }
            daApplication.Dispose();
            return lstApplication;
        }

        public  List<UsersApplicationsRoles> FindApplicationforUser(string strUser)
        {
            var daApplication = new ApplicationDA(_configuration);
            var lstAppUserRol = new List<UsersApplicationsRoles>();

            var ds = daApplication.FindApplicationforUser(strUser);

            if (ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow item in ds.Tables[0].Rows)
                {
                    var objAppUserRole = new UsersApplicationsRoles
                                             {
                                                 ApplicationId = Convert.ToInt32(item[0]),
                                                 ApplicationName = item[1].ToString(),
                                                 ApplicationDescription = item[2].ToString()
                                             };

                    if (!item[3].ToString().Equals(string.Empty))
                    { objAppUserRole.ValidityStartDate = string.Format("{0:dd/MM/yyyy}", Convert.ToDateTime(item[3])); }
                    
                    if (!item[4].ToString().Equals(string.Empty))
                    { objAppUserRole.DeclineDate = string.Format("{0:dd/MM/yyyy}", Convert.ToDateTime(item[4])); }
                    objAppUserRole.Observations = item[5].ToString();
                    objAppUserRole.EmployeeNames =item[6].ToString();

                    lstAppUserRol.Add(objAppUserRole);
                }
            }

            return lstAppUserRol;
        }

        public  object [] GetApplicationAdministration(ApplicationPMX application)
        {
            var applicationDa = new ApplicationDA(_configuration);
            var applicationAdmin = applicationDa.GetApplicationAdministration(application);
            applicationDa.Dispose();
            return applicationAdmin;
        }

        public void usuariosModificados(User registerUser, string mensaje)
        {
            var logCuentasUsuarios = new Log
                        {
                            Application = new ApplicationPMX { ApplicationName = _applicationName },
                            EventUser = registerUser,
                            EventTypeId = LogTypeEnum.Notification,
                            LogDescription = string.Format(mensaje)
                        };
            var loglogic = new LogLogic(_configuration);
            loglogic.InsertLogEvent(logCuentasUsuarios);
            loglogic.Dispose();
        }



        public List<CuentasRolesAsignados> SearchCuentasRolesLogic(int idAplicacion)
        {
            var daApplication = new ApplicationDA(_configuration);
            var lstApplication = new List<CuentasRolesAsignados>();

            var ds = daApplication.SearchCuentasRolesData(idAplicacion);

            if (ds.Tables[0].Rows.Count > 0)
            {
                lstApplication.AddRange(from DataRow item in ds.Tables[0].Rows
                                        select new CuentasRolesAsignados
                                        {
                                            ApplicationId = Convert.ToInt32(item[1].ToString()),
                                            Cuenta = item[3].ToString(),
                                            Ficha = item[4].ToString(),
                                            Nombres = item[5].ToString() + " " + item[6].ToString(),
                                            Rol = item[2].ToString()
                                        });
            }

            return lstApplication;
        }

        public List<CuentasSistemaRol> SearchCuentasSistemaRolLogic(int cuentaUsuario)
        {
            var daApplication = new ApplicationDA(_configuration);
            var lstApplication = new List<CuentasSistemaRol>();

            var ds = daApplication.SearchCuentasSistemaRol(cuentaUsuario);

            if (ds.Tables[0].Rows.Count > 0)
            {
                lstApplication.AddRange(from DataRow item in ds.Tables[0].Rows
                                        select new CuentasSistemaRol
                                        {
                                          UserId = item[0].ToString(),
                                          EmployeeNumber = item[1].ToString(),
                                          EmployeeNames = item[2].ToString() + " " + item[3].ToString(),
                                          DeclineDateR = Convert.ToDateTime(item[4].ToString()),
                                          DeclineDateU = Convert.ToDateTime(item[5].ToString())
                                        });
            }

            return lstApplication;
        }


        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        #endregion
    } 
}
