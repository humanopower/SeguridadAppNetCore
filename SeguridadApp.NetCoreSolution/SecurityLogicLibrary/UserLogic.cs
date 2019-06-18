using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using EntityLibrary;
using System.Data;
using System.Collections.Generic;
using DataAccessSecurity;
using SecurityLogicLibraryContracts;
using Microsoft.Extensions.Configuration;

namespace SecurityLogicLibrary
{
    public class UserLogic : IUserLogic
    {

		#region Atributes
		private IConfiguration _configuration;
		private string _applicationName;
		#endregion
		#region Constructor
		public UserLogic(IConfiguration configuration)
		{
			_configuration = configuration;
			_applicationName = _configuration.GetConnectionString("SecurityConnectionString");
		}
		#endregion
		#region Metodos Publicos
		public Response AddNewUser(EntityLibrary.User user, EntityLibrary.User registerUser)
        {
             var response = new Response{Message="Sin inicializar",Result = false};
            try
            {


                #region AddUserDataValidation
                if(string.IsNullOrEmpty(user.UserId))
                {
                    response.Message = "El usuario no puede estar vacío";
                    return response;
                }

                if(string.IsNullOrEmpty(user.EmployeeNumber))
                {
                    response.Message = "El campo ficha no puede estar vacío";
                    return response;
                }
                
                /* 06/10/2015 Se deshabilita validación debido a que las cuentas de servicio no son necesariamente numéricas */
                /*
                int employeenumber;
                var isNum = Int32.TryParse(user.EmployeeNumber, out employeenumber);
                if(!isNum)
                {

                    response.Message = "El campo ficha solo acepta valores numericos";
                    return response;
                    
                }
                */

                if(string.IsNullOrEmpty(user.EmployeeNames))
                {
                    response.Message = "El campo nombre no puede estar vacío";
                    return response;
                }

                if(string.IsNullOrEmpty(user.EmployeeLastName))
                {
                    response.Message = "El campo apellido paterno no puede estar vacío";
                    return response;
                }



                if(string.IsNullOrEmpty(user.EmployeeEmail))
                {
                    response.Message = "El campo correo electronico no puede estar vacío";
                    return response;
                }
                if(string.IsNullOrEmpty(user.Telephone))
                {
                    response.Message = "El campo telefono no puede estar vacío";
                    return response;
                }

                if(Convert.ToDateTime(user.ValidityStartDate).Year != DateTime.Now.Year)
                {
                    response.Message = "El año de inicio de vigencia no puede ser distinto al actual. ";
                    return response;
                }

                if (registerUser == null)
               {
                   response.Message = "No se ha especificado el usuario con permisos para registrar";
                   return response;
               }
              #endregion

                var userDa = new UserDA();
                userDa.AddUser(user, registerUser);
              userDa.Dispose();

                #region logRegister

               var log = new Log
               {
                   Application = new ApplicationPMX
                   {
                       ApplicationName = _applicationName
                   },
                   EventUser = registerUser,
                   EventTypeId = LogTypeEnum.Notification,
                   LogDescription = string.Format("Agregó la cuenta {0} asignado a F-{1} - {2}, vigente a partir de {3} Vigente hasta {4} vigente en SIO hasta {5}. Observaciones: {6} ",
                                                user.UserId,user.EmployeeNumber, user.EmployeeNames+ " "+user.EmployeeLastName, user.ValidityStartDate, user.DeclineDate, user.DeclineDateSIO,user.Observations)
               };

               #endregion
               var loglogic = new LogLogic(_configuration);
               loglogic.InsertLogEvent(log);
               loglogic.Dispose();
              

                response.Message = "Se registró correctamente el usuario para uso de aplicaciones.";
                response.Result = true;
            }
            catch (Exception err)
            {
               if (err.Message.Substring(0,35) == "Violation of PRIMARY KEY constraint")
                {
                    var log = new Log
                                  {
                                      EventUser = user,
                                      EventTypeId = LogTypeEnum.Notification,
                                      LogDescription = string.Format("Se intentó agregar al usuario {0} al esquema de seguridad, pero ya existe previamente",user.UserId),
                                      Application = new ApplicationPMX
                                                        {
                                                            ApplicationName =
                                                                _applicationName
                                                        } 
                                  };
                    var loglogic = new LogLogic(_configuration);
                    loglogic.InsertLogEvent(log);
                    loglogic.Dispose();
                    response.Message ="La cuenta que ha intentado agregar ha sido registrada previamente.";
                    return response;
                }
               response.Message = string.Format("Ocurrio un error al intentar agregar el usuario. {0} {1}", err.Message, DateTime.Now.ToString(CultureInfo.InvariantCulture));
                return response; 
            }
            return response;
        }

        public  Response DelUser(User user, User registerUser)
        {
            var response = new Response { Message = "Sin inicializar", Result = false };
            try
            {
               

                #region AddUserDataValidation
                if (string.IsNullOrEmpty(user.UserId))
                {
                    response.Message = "El usuario no puede estar vacío";
                    return response;
                }

                if (string.IsNullOrEmpty(user.EmployeeNumber))
                {
                    response.Message = "El campo ficha no puede estar vacío";
                    return response;
                }

                //if (Convert.ToDateTime(user.DeclineDate).Year != DateTime.Now.Year)
                //{
                //    response.Message = "El año de inicio de vigencia no puede ser distinto al actual. ";
                //    return response;
                //}

                if (registerUser == null)
                {
                    response.Message = "No se ha especificado el usuario con permisos para registrar";
                    return response;
                }
                #endregion
                var userDa = new UserDA();
                userDa.DelUser(user, registerUser);
                userDa.Dispose();
                #region logRegister

                var log = new Log
                {
                    Application = new ApplicationPMX
                    {
                        ApplicationName =
                            _applicationName
                    },
                    EventUser = registerUser,
                    EventTypeId = LogTypeEnum.Notification,
                    LogDescription = string.Format("Declinó la cuenta {0} asignado a F-{1} - {2}, Vigente hasta {3} ",
                                                 user.UserId, user.EmployeeNumber, user.EmployeeNames + " " + user.EmployeeLastName,  user.DeclineDate)

                };
                #endregion
                var loglogic = new LogLogic(_configuration);
               loglogic.InsertLogEvent(log);
                loglogic.Dispose();
               


                response.Message = "Se declino correctamente el usuario para uso de aplicaciones.";
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
                        LogDescription = string.Format("Se intentó declinar el usuario {0} del esquema de seguridad, pero ya existe previamente", user.UserId),
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
                response.Message = string.Format("Ocurrio un error al intentar declinar el usuario. {0} {1}", err.Message, DateTime.Now.ToString(CultureInfo.InvariantCulture));
                return response;
            }
            return response;
        }

        public Response UpdUser(User user, User registerUser)
        {
            var response = new Response { Message = "Sin inicializar", Result = false };
            try
            {
                

                #region AddUserDataValidation
                if (string.IsNullOrEmpty(user.UserId))
                {
                    response.Message = "El usuario no puede estar vacío";
                    return response;
                }

                if (string.IsNullOrEmpty(user.EmployeeNumber))
                {
                    response.Message = "El campo ficha no puede estar vacío";
                    return response;
                }

                int employeenumber;
                bool isNum = Int32.TryParse(user.EmployeeNumber, out employeenumber);
                if (!isNum)
                {
                    response.Message = "El campo ficha solo acepta valores numericos";
                    return response;
                }

                if (string.IsNullOrEmpty(user.EmployeeNames))
                {
                    response.Message = "El campo nombre no puede estar vacío";
                    return response;
                }

                if (string.IsNullOrEmpty(user.EmployeeLastName))
                {
                    response.Message = "El campo apellido paterno no puede estar vacío";
                    return response;
                }

                if (string.IsNullOrEmpty(user.EmployeeEmail))
                {
                    response.Message = "El campo correo electronico no puede estar vacío";
                    return response;
                }

                if (string.IsNullOrEmpty(user.Telephone))
                {
                    response.Message = "El campo telefono no puede estar vacío";
                    return response;
                }

                if (Convert.ToDateTime(user.ValidityStartDate).Year != DateTime.Now.Year)
                {
                    response.Message = "El año de inicio de vigencia no puede ser distinto al actual. ";
                    return response;
                }

                if (registerUser == null)
                {
                    response.Message = "No se ha especificado el usuario con permisos para registrar";
                    return response;
                }
                #endregion

                UserDA userDa = new UserDA();
                userDa.UpdUser(user, registerUser);
                userDa.Dispose();
                #region logRegister

                var log = new Log
                {
                    Application = new ApplicationPMX
                    {
                        ApplicationName =
                            _applicationName
                    },
                    EventUser = registerUser,
                    EventTypeId = LogTypeEnum.Notification,
                    LogDescription = string.Format("Actualizó la cuenta {0} asignado a F-{1} - {2}, vigente a partir de {3} Vigente hasta {4} vigente en SIO hasta {5}. Observaciones: {6} ",
                                                 user.UserId, user.EmployeeNumber, user.EmployeeNames + " " + user.EmployeeLastName, user.ValidityStartDate, user.DeclineDate, user.DeclineDateSIO, user.Observations)

                };
                #endregion

                var loglogic = new LogLogic(_configuration);
                loglogic.InsertLogEvent(log);
                loglogic.Dispose();
               
                 

                response.Message = "Se actualizo correctamente el usuario para uso de aplicaciones.";
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
                        LogDescription = string.Format("Se intentó agregar al usuario {0} al esquema de seguridad, pero ya existe previamente", user.UserId),
                        Application = new ApplicationPMX
                        {
                            ApplicationName =
                                _applicationName
                        }
                    };
                    var loglogic = new LogLogic(_configuration);
                    loglogic.InsertLogEvent(log);
                    loglogic.Dispose();
                    response.Message = "El usuario ha sido registrado previamente";
                    return response;
                }
                response.Message = string.Format("Ocurrio un error al intentar agregar el usuario. {0} {1}", err.Message, DateTime.Now.ToString(CultureInfo.InvariantCulture));
                return response;
            }
            return response;
        }

        public User FindUser(string userId)
        {
            User existingUser = null;
            
            try
            {
                var userDa = new UserDA();
                existingUser = userDa.FindUser(userId);
                userDa.Dispose();
            }
            catch (Exception ex)
            {

                existingUser = null;
            }
            return existingUser;
        }

        /// <summary>
        /// Metodo que regresa una lista de registros que coinciden con la búsqueda
        /// </summary>
        /// <param name="strValue">Valor a buscar</param>
        /// <returns>Regresa un objeto lista de los usuarios</returns>
        public List<User> SearchUser(string strValue,User registerUser)
        {
            var daUser = new UserDA();
            var ds = new DataSet();
            var lstUser = new List<User>();

            ds = daUser.FindUsers(strValue, registerUser);

            if (ds.Tables[0].Rows.Count  > 0)
            {
                lstUser.AddRange(from DataRow item in ds.Tables[0].Rows
                                 select new User
                                            {
                                                UserId = item[0].ToString(), 
                                                EmployeeNumber = item[1].ToString(), 
                                                EmployeeNames = item[2].ToString(), 
                                                EmployeeLastName = item[3].ToString(),
                                                Telephone = item[4].ToString(),
                                                MobileTelephone = item[5].ToString(),
                                                ValidityStartDate = string.IsNullOrEmpty(item[6].ToString()) ? string.Empty : string.Format("{0:dd/MM/yyy}",item[6]), 
                                                DeclineDate = string.IsNullOrEmpty(item[7].ToString()) ? string.Empty : string.Format("{0:dd/MM/yyy}",item[7]),  
                                                DeclineDateSIO = string.IsNullOrEmpty(item[8].ToString()) ? string.Empty : string.Format("{0:dd/MM/yyy}",item[8])
                                            });
            }

            return lstUser;
        }


        public List<User> SearchUserScope(string strValue, User registerUser)
        {
            var daUser = new UserDA();
            var ds = new DataSet();
            var lstUser = new List<User>();

            ds = daUser.FindUsersScope(strValue, registerUser);

            if (ds.Tables[0].Rows.Count > 0)
            {
                lstUser.AddRange(from DataRow item in ds.Tables[0].Rows
                                 select new User
                                 {
                                     UserId = item[0].ToString(),
                                     EmployeeNumber = item[1].ToString(),
                                     EmployeeNames = item[2].ToString(),
                                     EmployeeLastName = item[3].ToString(),
                                     Telephone = item[4].ToString(),
                                     MobileTelephone = item[5].ToString(),
                                     ValidityStartDate = string.IsNullOrEmpty(item[6].ToString()) ? string.Empty : string.Format("{0:dd/MM/yyy}", item[6]),
                                     DeclineDate = string.IsNullOrEmpty(item[7].ToString()) ? string.Empty : string.Format("{0:dd/MM/yyy}", item[7]),
                                     DeclineDateSIO = string.IsNullOrEmpty(item[8].ToString()) ? string.Empty : string.Format("{0:dd/MM/yyy}", item[8])
                                 });
            }

            return lstUser;
        }

        public  List<User> GetUserId(string strValue)
        {
            var userDa = new UserDA();
            var listUser = userDa.GetUser(strValue);
            userDa.Dispose();
            return listUser;
        }

        public string GetNameUserAccount(string numeroEmpleado)
        {
            UserDA userDa = new UserDA();
            string nombreCompleto = userDa.GetNameUserAccount(numeroEmpleado);
            userDa.Dispose();
            return nombreCompleto;
        }


        #endregion

        public void Dispose()
        {
           GC.SuppressFinalize(this);
        }
    }
}
