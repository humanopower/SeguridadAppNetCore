using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using EntityLibrary;
using DataAccessSecurity;
using System.Globalization;
using SecurityLogicLibraryContracts;
using Microsoft.Extensions.Configuration;

namespace SecurityLogicLibrary
{
    public class RoleOperacionLogic : IRoleOperacionLogic
    {
		#region Atributes
		private IConfiguration _configuration;
		private string _applicationName;
		#endregion
		#region Constructor
		public RoleOperacionLogic(IConfiguration configuration)
		{
			_configuration = configuration;
			_applicationName = _configuration.GetConnectionString("SecurityConnectionString");
		}
		#endregion

		#region Metodos Publicos
		public Response AddNewRoleOperation(RoleOperations roleOperations, User registerUser)
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
                    
                    /*LogDescription = string.Format("Se ha agregado la operacion {0} al rol {1}.", roleOperations.OperationId, roleOperations.RoleId)*/
                    LogDescription = string.Format("Agregó la operacion Id {0} al rol Id {1} Fecha vigencia: {2}", roleOperations.OperationId, roleOperations.RoleId, roleOperations.DeclineDate)
                    
                };
                #endregion

                #region AddUserDataValidation
                if (roleOperations.RoleId <= 0)
                {
                    response.Message = "El campo role no puede estar vacío";
                    return response;
                }

                if (roleOperations.OperationId <= 0)
                {
                    response.Message = "El campo Operación no puede estar vacío";
                    return response;
                }
                
                if (registerUser == null)
                {
                    response.Message = "No se ha especificado el usuario con permisos para registrar";
                    return response;
                }


                //Se valida que no exista la prohibición de combinacion de operaciones
                var operationlogic = new OperationLogic(_configuration);
                var role = new Role {RoleId = roleOperations.RoleId};
                Operation operationB = operationlogic.GetOperationById(roleOperations.OperationId);
                var applicationLogic = new ApplicationLogic(_configuration);
                ApplicationPMX application = applicationLogic.GetApplication(operationB.ApplicationId);
                applicationLogic.Dispose();
                /*Aqui hay que hacer algo*/
                
                List<Operation> operations = operationlogic.GetOperationRole(role);
              
                foreach (var operationA in operations)
                {
                    var resultExist = operationlogic.OperationsNotAllowedCombinationExistAndDate(application, operationA, operationB);

                    if(resultExist.Result)
                    {
                        response.Message =
                            "No se puede agregar operación al rol, ya que no está permitida en la combinación de operaciones";
                        return response;
                    }
                }
                operationlogic.Dispose();
              

                #endregion
                var loglogic = new LogLogic(_configuration);
                var responseLog = loglogic.InsertLogEvent(log);
                loglogic.Dispose();

               
                if (!responseLog.Result)
                {
                    response.Message = string.Format("No se puede insertar en bitácorá el movimiento. {0}", responseLog.Message);
                    return response;
                }
                
                //Validamos que no este asignada la operación al role
                var roleLogic = new RoleLogic(_configuration);
                var lstOperacion = roleLogic.GetRoleOperation(roleOperations.RoleId);
                roleLogic.Dispose();
                var iRes = 0;

                if (lstOperacion.Any(operation => operation.OperationId.Equals(roleOperations.OperationId)))
                {
                    iRes = 1;
                }

                if (iRes <= 0)
                {
                    var roleOperationDa = new RoleOperationDA(_configuration);
                    roleOperationDa.AddNewRoleOperation(roleOperations, registerUser);
                    roleOperationDa.Dispose();
                    response.Message = "Se asocio correctamente la operación {0} al rol {1}";
                    response.Result = true;
                }
                else
                {
                    response.Message = "La operación ya esta asignada a ese rol, favor de verificar.";
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
                        LogDescription = string.Format("Se intentó asignar la operación al role  {0} en el esquema de seguridad, pero ya existe previamente", roleOperations.RoleId),
                        Application = new ApplicationPMX
                        {
                            ApplicationName =
                                _applicationName
                        }
                    };
                    var loglogic = new LogLogic(_configuration);
                    loglogic.InsertLogEvent(log);
                    loglogic.Dispose();
                    response.Message = "El rol ha sido registrado previamente";
                    return response;
                }
                response.Message = string.Format("Ocurrio un error al intentar asignar el rol. {0} {1}", err.Message, DateTime.Now.ToString(CultureInfo.InvariantCulture));
                return response;
            }
            return response;
        }

        public  Response DelRoleOperation(RoleOperations roleOperations, User registerUser)
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
                  
                    LogDescription = string.Format("Declinó la operacion Id {0} al rol Id {1} Fecha vigencia: {2}", roleOperations.OperationId, roleOperations.RoleId, roleOperations.DeclineDate)
              };
                #endregion

                #region AddUserDataValidation
                if (roleOperations.RoleId <= 0)
                {
                    response.Message = "El campo role no puede estar vacío";
                    return response;
                }

                if (roleOperations.OperationId <= 0)
                {
                    response.Message = "El campo Operación no puede estar vacío";
                    return response;
                }

                if (registerUser == null)
                {
                    response.Message = "No se ha especificado el usuario con permisos para registrar";
                    return response;
                }
                #endregion

                var loglogic = new LogLogic(_configuration);
               var responseLog =  loglogic.InsertLogEvent(log);
                loglogic.Dispose();
                if (!responseLog.Result)
                {
                    response.Message = string.Format("No se puede insertar en bitácorá el movimiento. {0}", responseLog.Message);
                    return response;
                }
                var roleOperationDa = new RoleOperationDA(_configuration);
                roleOperationDa.DelRoleOperation(roleOperations, registerUser);
                roleOperationDa.Dispose();
                response.Message = "Se declino correctamente el rol para uso de operaciones.";
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
                        LogDescription = string.Format("Se intentó agregar al usuario {0} al esquema de seguridad, pero ya existe previamente", roleOperations.RoleId),
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

        public  List<Operation>GetOperationRole(Role role)
        {
            var roleOperationDa = new RoleOperationDA(_configuration);
            var listOperation = roleOperationDa.GetoperationRole(role);
            roleOperationDa.Dispose();
            return listOperation;
        }

        public  List<Role>GetOperationsList(Role roles)
        {
            var lstRole = new List<Role>();
            var roleOperationDa = new RoleOperationDA(_configuration);
            var operationDa = new OperationDA(_configuration);
            var lstRoleoperation = roleOperationDa.GetRoleOperations(roles);

            if (lstRoleoperation.Count>0)
            {
                var role = new Role();
                var roleLogic = new RoleLogic(_configuration);
                foreach (var itemroleOperationse in lstRoleoperation)
                {
                    if (role.RoleId!=itemroleOperationse.RoleId)
                    {
                        role.RoleId = itemroleOperationse.RoleId;

                        role = roleLogic.GetRole(role);

                        role.OperationsList = new List<Operation>();

                        var lstoperation = operationDa.GetRoleOperations(role);
                       

                        role.OperationsList = lstoperation;

                        lstRole.Add(role);
                    }
                }
                roleLogic.Dispose();
            }
            roleOperationDa.Dispose();
            operationDa.Dispose();

            return lstRole;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        #endregion

     
    }
}
