using System;
using System.Collections.Generic;
using DataAccessSecurity;
using EntityLibrary;
using Microsoft.Extensions.Configuration;
using SecurityLogicLibraryContracts;

namespace SecurityLogicLibrary
{
    public class LogLogic : ILogLogic
    {
		#region Atributes
		private IConfiguration _configuration;
		private string _applicationName;
		#endregion
		#region Constructor
		public LogLogic(IConfiguration configuration)
		{
			_configuration = configuration;
			_applicationName = _configuration.GetConnectionString("SecurityConnectionString");
		}
		#endregion

		public Response InsertLogEvent(Log log)
        {
            var response = new Response { Result = false, Message = "Sin inicializar"};
            try
            {
                if (log.Application == null)
                {
                    response.Message = "Debe de inicializar el objeto Application.";
                    return response;
                }
                var applicationDA = new ApplicationDA(_configuration);
                var applicationDbList = applicationDA.GetApplicationList();
                applicationDA.Dispose();

                var appFinded = applicationDbList.Find(app => app.ApplicationName == log.Application.ApplicationName);
                if(appFinded == null)
                {
                    response.Message = "La aplicación no existe en SeguridadApp.";
                    return response;
                }
                log.Application = appFinded;

                if(log.EventUser == null)
                {
                    response.Message = "Debe de inicializar el objeto EventUser";
                    return response;
                    
                }

                string[] usuarioEncontrar = log.EventUser.UserId.Split(' ');
                var userLogic = new UserLogic(_configuration);
                User userFinded = userLogic.FindUser(usuarioEncontrar[0]);
                userLogic.Dispose();
                if(userFinded == null)
                {
                    response.Message = "La cuenta de usuario no existe en SeguridadApp.";
                    return response;
                }
                log.EventUser = userFinded;

                var logDa = new LogDA(_configuration);
                logDa.AddLogEvent(log);
                logDa.Dispose();

                response.Message = "Se registró correctamente el evento en la bitácora";
                response.Result = true;
                return response;
            }
            catch (Exception err)
            {

                response.Message = string.Format("Ocurrió un error al intentar realizar el registro en bitácora. {0}", err.Message);
                return response;
            }
           
        }

        public  List<Log> GetLogList()
        {
            var logDA = new LogDA(_configuration);
            var listLog = logDA.GetLogList();
            logDA.Dispose();
            return listLog;
        }

        public void Dispose()
        {
           GC.SuppressFinalize(this);
        }
    }
}
