using System;
using System.Web;
using DataAccessSecurity;
using EntityLibrary;
using Microsoft.Extensions.Configuration;
using SecurityLogicLibraryContracts;

namespace SecurityLogicLibrary
{
    public class SessionLogic : ISessionLogic
    {
		#region Atributes
		private IConfiguration _configuration;
		private string _applicationName;
		#endregion
		#region Constructor
		public SessionLogic(IConfiguration configuration)
		{
			_configuration = configuration;
			_applicationName = _configuration.GetConnectionString("SecurityConnectionString");
		}
		#endregion
		public Response AddSession(User userAuthenticated, ApplicationPMX application)
        {
            Response response = new Response {Message = "Sin inicializar", Result = false};
            try
            {
                var sessionDataAccess = new SessionsDataAccess(_configuration);
                sessionDataAccess.AddSession(userAuthenticated, application);
                sessionDataAccess.Dispose();
                
                var log = new Log
                {
                    Application = application,
                    EventUser = userAuthenticated,
                    EventTypeId = LogTypeEnum.LogInSuccesful,
                   //EventIpAddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ??
                   //                  HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"],
                    LogDescription = string.Format("Se ha agregado la sesión {0} del usuario {1} al esquema de seguridad.", userAuthenticated.SessionId, userAuthenticated.UserId)
                };

                var loglogic = new LogLogic(_configuration);
                loglogic.InsertLogEvent(log);
                loglogic.Dispose();

                 response.Message = "Se agrego exitosamente la sesión al esquema de seguridad.";
               
                response.Result = true;
                return response;
            }
            catch (Exception err)
            {
                
                response.Message = string.Format("Ocurrió un error al intentar crear una sesión en el esquema de seguridad. {0}",err.Message);
                return response;
            }

        }

        public Response ValidateSession(User userAuthenticaded, ApplicationPMX appFinded)
        {
            var response = new Response {Message="Not initialized",Result = false};
            var sessionDataAccess = new SessionsDataAccess(_configuration);
            try
            {
                bool sessionFinded;
                bool isSessionValid;
               
                sessionDataAccess.FindSession(userAuthenticaded, appFinded, out sessionFinded, out isSessionValid);
               
                
                if(sessionFinded)
                {
                   if (isSessionValid)
                   {
                       sessionDataAccess.UpdateSessionEndTime(userAuthenticaded, appFinded);
                       response.Message = "Sesión valida. Se actualizó tiempo de expiración.";
                       response.Result = true;
                       return response;
                   }
                   response.Message = "Tiempo de sesión expirado. Autentique nuevamente";
                    return response;
                }
                response.Message = "No se encontró la sesión especificada.";
               
              
            }
            catch (Exception err)
            {
                response.Message = string.Format("Ocurrio un error mientras se consultaba la sesión. Err. {0}",
                                                 err.Message);
               
            }
           
            sessionDataAccess.Dispose();
            return response;
        }

        public Response FindUserBySession(string sessionGuid, out User userFinded)
        {
            var response = new Response { Message = "Not initialized", Result = false };
            userFinded = new User();
            var sessionDataAccess = new SessionsDataAccess(_configuration);
            try
            {


                string userId = sessionDataAccess.FindSessionUser(sessionGuid);

               if (!string.IsNullOrEmpty(userId))
               {
                   var userLogic = new UserLogic(_configuration);
                   userFinded = userLogic.FindUser(userId);
                   userLogic.Dispose();
                    userFinded.SessionId = new Guid(sessionGuid);
                    response.Result = true;
                    response.Message = "Se encontró la sesión buscada";
                }
                else
                {
                    response.Message = "La sesión proporcionada no es válida";
                }
               
            }
            catch (Exception err)
            {
                response.Message = string.Format("Ocurrio un error mientras se consultaba la sesión. Err. {0}",
                                                 err.Message);
               
            }
           
            sessionDataAccess.Dispose();
            return response;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
