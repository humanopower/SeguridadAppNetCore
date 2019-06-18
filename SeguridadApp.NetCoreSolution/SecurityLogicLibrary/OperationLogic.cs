using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using DataAccessSecurity;
using EntityLibrary;
using Microsoft.Extensions.Configuration;
using SecurityLogicLibraryContracts;

namespace SecurityLogicLibrary
{
    public class OperationLogic : IOperationLogic
    {
		#region Atributes
		private IConfiguration _configuration;
		private string _applicationName;
		#endregion
		#region Constructor
		public OperationLogic(IConfiguration configuration)
		{
			_configuration = configuration;
			_applicationName = _configuration.GetConnectionString("SecurityConnectionString");
		}
		#endregion
		public Response AddNewOperation(Operation operation, User registerUser)
        {
            var response = new Response { Message = "Sin inicializar", Result = false };
            var operationDA = new OperationDA(_configuration);
            try
            {

                
                #region AddOperationDataValidation
                if (operation.ApplicationId == 0)
                {
                    response.Message = "El IdAplicacion  no puede estar vacío";
                    return response;
                }

                if (string.IsNullOrEmpty(operation.OperationName))
                {
                    response.Message = "El campo nombre no puede estar vacío";
                    return response;
                }

                if (string.IsNullOrEmpty(operation.OperationDescription))
                {
                    response.Message = "El campo Descripción no puede estar vacío";
                    return response;
                }

                if (registerUser == null)
                {
                    response.Message = "No se ha especificado el usuario con permisos para registrar";
                    return response;
                }
                #endregion



                //Validamos Operation
                var objOperations = operationDA.GetDataByOperationName(operation);

               if (objOperations !=null)
                {
                    response.Message = "El nombre de la operación ya existe, favor de verificarlo.";
                    return response;
                }

               operationDA.AddOperation(operation, registerUser);
                #region logRegister
                var log = new Log
                {
                    Application = new ApplicationPMX
                    {
                        ApplicationName = _applicationName
                    },
                    EventUser = registerUser,
                    EventTypeId = LogTypeEnum.Notification,
                    LogDescription = string.Format("Agregó la operación {0}-{1} al esquema de seguridad. Descripción: {2} Fecha declinacion {3}  ", operation.OperationId, operation.OperationName, operation.OperationDescription, operation.DeclineDate)
                };

                #endregion

                var loglogic = new LogLogic(_configuration);
                loglogic.InsertLogEvent(log);
                loglogic.Dispose();
                

                response.Message = string.Format("Se registró correctamente la operación {0} ",operation.OperationName);

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
                        LogDescription = string.Format("Se intentó agregar la operación {0} al esquema de seguridad, pero ya existe previamente", operation.OperationName),
                        Application = new ApplicationPMX
                        {
                            ApplicationName = _applicationName
                        }
                    };
                    var loglogic = new LogLogic(_configuration);
                    loglogic.InsertLogEvent(log);
                    loglogic.Dispose();
                    response.Message = "La operación ha sido registrada previamente";
                    return response;
                }
                response.Message = string.Format("Ocurrio un error al intentar agregar la Operación. {0} {1}", err.Message, DateTime.Now.ToString(CultureInfo.InvariantCulture));
               
            }
            operationDA.Dispose();
            return response;
        }

        public  Response UpdOperation(Operation operation, User registerUser)
        {
            var response = new Response { Message = "Sin inicializar", Result = false };
            var operationDA = new OperationDA(_configuration);
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
                    LogDescription = string.Format("Actualizó la operación {0}-{1} al esquema de seguridad. Descripción: {2} Fecha declinacion {3}  ", operation.OperationId, operation.OperationName, operation.OperationDescription, operation.DeclineDate)

                };

                #endregion

                #region AddOperationDataValidation
                if (operation.ApplicationId == 0)
                {
                    response.Message = "El IdAplicacion  no puede estar vacío";
                    return response;
                }

                if (string.IsNullOrEmpty(operation.OperationName))
                {
                    response.Message = "El campo nombre no puede estar vacío";
                    return response;
                }

                if (string.IsNullOrEmpty(operation.OperationDescription))
                {
                    response.Message = "El campo Descripción no puede estar vacío";
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

                operationDA.UpdOperation(operation, registerUser);

                response.Message = "Se atualizó correctamente la operación para uso de Roles.";
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
                        LogDescription = string.Format("Se intentó agregar la operación {0} al esquema de seguridad, pero ya existe previamente", operation.OperationName),
                        Application = new ApplicationPMX
                        {
                            ApplicationName = _applicationName
                        }
                    };
                    var loglogic = new LogLogic(_configuration);
                    loglogic.InsertLogEvent(log);
                    loglogic.Dispose();

                    response.Message = "La operación ha sido registrada previamente";
                    return response;
                }
                response.Message = string.Format("Ocurrio un error al intentar agregar la Operación. {0} {1}", err.Message, DateTime.Now.ToString(CultureInfo.InvariantCulture));
              
            }
            operationDA.Dispose();
            return response;
        }

        public  Response DelOperation(Operation operation, User registerUser)
        {
            var response = new Response { Message = "Sin inicializar", Result = false };
            var operationDA = new OperationDA(_configuration);
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
                    LogDescription = string.Format("Declinó la operación {0}-{1} al esquema de seguridad. Descripción: {2} Fecha declinacion {3}  ", operation.OperationId, operation.OperationName, operation.OperationDescription, operation.DeclineDate)
                };

                #endregion

                #region AddOperationDataValidation
                if (operation.ApplicationId == 0)
                {
                    response.Message = "El IdAplicacion  no puede estar vacío";
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

                operationDA.DelOperation(operation, registerUser);

                response.Message = "Se declinó correctamente la operación.";
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
                        LogDescription = string.Format("Se intentó declinar la operación {0} al esquema de seguridad, pero ya existe previamente", operation.OperationName),
                        Application = new ApplicationPMX
                        {
                            ApplicationName = _applicationName
                        }
                    };
                    var loglogic = new LogLogic(_configuration);
                    var responseLog = loglogic.InsertLogEvent(log);
                    loglogic.Dispose();
                    response.Message = "La operación ha sido declinada previamente";
                    operationDA.Dispose();
                    return response;
                }
                response.Message = string.Format("Ocurrio un error al intentar agregar la Operación. {0} {1}", err.Message, DateTime.Now.ToString(CultureInfo.InvariantCulture));
              
            }
            operationDA.Dispose();
            return response;
        }

        public  List<Operation> GetOperationList(List<Role> listrole )
        {
            var operationDa = new OperationDA(_configuration);
            var listOperation = operationDa.GetOperationsList(listrole);
            operationDa.Dispose();
            return listOperation;
        }

        public  List<Operation> GetOperationList(ApplicationPMX application)
        {
            var operationDa = new OperationDA(_configuration);
            var listOperation = operationDa.GetOperationsList(application);
            operationDa.Dispose();
            return listOperation;
        }

        public  List<Operation> GetAllOperations()
        {
            var operationDa = new OperationDA(_configuration);
            var listOperation = operationDa.GetAllOperations();
            operationDa.Dispose();
            return listOperation;
        }

        public  List<Operation> GetOperationByItems(string strValue)
        {
            var operationDa = new OperationDA(_configuration);
            var listOperation = operationDa.GetOperationsForItems(strValue);
            operationDa.Dispose();
            return listOperation;
        }

        public  Operation GetOperationById(int operationId)
        {
            var operationDa = new OperationDA(_configuration);
            var operation=  operationDa.GetDataByIdOperation(operationId);
            operationDa.Dispose();
            return operation;
        }

        public  List<Operation> GetOperationRole(Role role)
        {
            var operationDa = new OperationDA(_configuration);

            var listOperation = operationDa.GetRoleOperations(role);
            operationDa.Dispose();
            return listOperation;
        }

        public Response AddOperationToApplication(ApplicationPMX application, Operation operation, User registerUser)
        {
            var response = new Response {Message = "No inicializado", Result = false};
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

                //OperationValidation
                if(string.IsNullOrEmpty(operation.OperationName) || operation.OperationName.Length < 7)
                {
                    response.Message =
                        string.Format("El nombre de la operación no puede estár vacío ni menor a 7 caracteres.");
                    return response;
                }
                var operationDA = new OperationDA(_configuration);
                var operationsList = operationDA.GetOperationsList(applicationFinded);
                var operationfinded = operationsList.Find(op => op.OperationName == operation.OperationName);
                if(operationfinded != null)
                {
                    response.Message = string.Format("La operacion {0} ya ha sido agregada previamente.", operation.OperationName);
                    return response;
                }

                
                //AddOperation
               
                operationDA.AddOperation(operation, userFinded);
                operationDA.Dispose();
                response.Result = true;
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
                        string.Format("Se agregó la operación {0} a la aplicación {1}.", operation.OperationName,
                                      applicationFinded.ApplicationName)

                };
                var loglogic = new LogLogic(_configuration);
                var resultLog = loglogic.InsertLogEvent(log);
                loglogic.Dispose();
                response.Message = string.Format(resultLog.Result ? "Se agregó la operación {0} a la aplicación {1}." : "Se agregó la operación {0} a la aplicación {1}. Pero no se pudo registrar el movimiento en bitácora.", operation.OperationName, applicationFinded.ApplicationName);
            }
            catch (Exception err)
            {

                response.Message = string.Format("Ocurrio un error. {0}", err.Message);
            }
            return response;
        }

        public Response UpdateRoleOperations(ApplicationPMX application, Role role, List<Operation> operationList, User registerUser, int tipoApp)
        {
            var response = new Response {Message = "Sin inicializar", Result = false};
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

                //Validate Role for application
                var userApplicationRole = new UsersApplicationsRoles
                                              {
                                                  ApplicationId = applicationFinded.ApplicationId
                                              };
                var roleDa = new RoleDA(_configuration);
                var roleList = roleDa.GetRoleforApplication(userApplicationRole, tipoApp);
                roleDa.Dispose();

                var roleFinded = roleList.Find(rol => rol.RoleName == role.RoleName);
                if(roleFinded == null)
                {
                    response.Message =
                        string.Format("El rol  {0} especificado no existe en el esquema de seguridad para la aplicación {1}.",role.RoleName, application.ApplicationName);
                    return response;
                }

                //Validate existing operations in applications and add operationProperties
                var operationDa = new OperationDA(_configuration);
                var appOperations = operationDa.GetOperationsList(applicationFinded);
                var operationsToAddList = new List<Operation>();
                foreach (var operation in operationList)
                {
                    var opfinded = appOperations.Find(op => op.OperationName == operation.OperationName);
                    if(opfinded == null)
                    {
                        response.Message = string.Format("La operación {0} especificada no existe en el esquema de seguridad para la aplicación {1}.", operation.OperationName, application.ApplicationName);
                        return response;
                    }
                    operationsToAddList.Add(opfinded);
                }
                //Delete all operations for role
                var currentOperations = operationDa.GetRoleOperations(roleFinded);
                operationDa.Dispose();

                foreach (var operationToDelete in from operationToDelete in currentOperations 
                                                  let deleteResponse = DeleteOperationToRole(applicationFinded, operationToDelete, roleFinded,userFinded) 
                                                  where !deleteResponse.Result select operationToDelete)
                {
                    response.Message =
                        string.Format("No se pudo actualizar la operación {0} en el esquema de seguridad.",
                                      operationToDelete.OperationName);
                    return response;

                }
                //Add all operations for role

                foreach (var operationToAdd in from operationToAdd in operationsToAddList 
                                               let addResponse = AddOperationToRole(applicationFinded,operationToAdd, roleFinded,userFinded) 
                                               where !addResponse.Result select operationToAdd)
                {
                    response.Message =
                        string.Format("No se pudo actualizar la operación {0} en el esquema de seguridad.",
                                      operationToAdd.OperationName);
                    return response;
                }

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
                        string.Format("Se actualizaron las operaciones del rol {0} de la aplicación {1}.",roleFinded.RoleName,
                                      applicationFinded.ApplicationName)

                };
                var loglogic = new LogLogic(_configuration);
                var resultLog = loglogic.InsertLogEvent(log);
                loglogic.Dispose();
                response.Message = string.Format(resultLog.Result ? "Se actualizaron las operaciones del rol {0} de la aplicación {1}." : "Se actualizaron las operaciones del rol {0} de la aplicación {1}. Pero no se pudo registrar el movimiento en bitácora.", roleFinded.RoleName, applicationFinded.ApplicationName);

                response.Result = true;

            }
            catch (Exception)
            {

                response.Message =
                    string.Format("Ocurrio un error al actualizar las operaciones del rol {0} de la aplicación {1}.",
                                  application.ApplicationName, registerUser);
            }
            return response;

        }

        public Response AddOperationToRole(ApplicationPMX application, Operation operation, Role role, User registerUser)
        {
            var response = new Response {Message = "Sin inicializar", Result = false};
            var operationDa = new OperationDA(_configuration);
            try
            {
                operationDa.AddOperationToRole(operation, role, registerUser);
                response.Message = string.Format("Se agregó la operacion {0} al rol {1}",operation.OperationName,role.RoleName);
                var log = new Log
                {
                    Application = application
                    ,
                    EventTypeId = LogTypeEnum.Notification
                    ,
                    EventUser = registerUser
                    ,
                    LogDescription =string.Format("Se agregó la operacion {0} al rol {1}",operation.OperationName,role.RoleName)
                        
                };

                var loglogic = new LogLogic(_configuration);
                loglogic.InsertLogEvent(log);
                loglogic.Dispose();
                response.Result = true;
            }
            catch (Exception)
            {
                response.Message = string.Format("Ocurrio un error al intentar insertar operacion {0} al rol {1}",
                                                 operation.OperationName, role.RoleName);
            }

           return  response;
        }

        public Response DeleteOperationToRole(ApplicationPMX application, Operation operation, Role role, User registerUser)
        {

            var response = new Response { Message = "Sin inicializar", Result = false };
            var operationsDA = new OperationDA(_configuration);
            try
            {
                operationsDA.DeleteOperationToRole(operation, role);
                response.Message = string.Format("Se eliminó la operacion {0} al rol {1}", operation.OperationName, role.RoleName);
                var log = new Log
                {
                    Application = application
                    ,
                    EventTypeId = LogTypeEnum.Notification
                    ,
                    EventUser = registerUser
                    ,
                    LogDescription = string.Format("Se eliminó la operacion {0} al rol {1}", operation.OperationName, role.RoleName)

                };

                var loglogic = new LogLogic(_configuration);
                loglogic.InsertLogEvent(log);
                loglogic.Dispose();
                response.Result = true;
            }
            catch (Exception)
            {

                response.Message = string.Format("Ocurrio un error al intentar eliminar operacion {0} al rol {1}",
                                                 operation.OperationName, role.RoleName);
            }
            operationsDA.Dispose();
            return response;
        }

        //public static Response CanCombineOperationsInRole(ApplicationPMX application, Operation operationA, Operation operationB)
        //{
        //    var response = new Response() {Message = "Not Initialized", Result = false};
        //    var resultExist = OperationsNotAllowedCombinationExist(application, operationA, operationB);
        //    if (resultExist.Result)
        //    {
        //        response.Message = string.Format("No se puede combinar las operaciones.{0}", resultExist.Message);
        //        response.Result = false;
        //        return response;
        //    }


        //    response.Message = "Se puede combinar las operaciones no permitidas";
        //    response.Result = true;
        //    return response;
        //}


        public  Response GetOperationsCombinationNotAllowedByApplication(ApplicationPMX application, out DataTable operationsCombinationsNotAllowed)
        {
            var response = new Response { Result = false, Message = "Not initialized" };
            operationsCombinationsNotAllowed = new DataTable();
            var operationDa = new OperationDA(_configuration);
            try
            {
                operationsCombinationsNotAllowed = operationDa.GetOperationsCombinationsNotAllowed(application);
                response.Message =
                    string.Format("Se encontraron {0} combinaciones no permitidas para la aplicación {1}.",
                                  operationsCombinationsNotAllowed.Rows.Count.ToString(), application.ApplicationName);
                response.Result = true;

            }
            catch (Exception e)
            {

                response.Message =
                    string.Format("Ocurrió un error al obtener las combinaciones de operaciones no autorizadas. {0}",
                                  e.Message);
                response.Result = false;
            }
            operationDa.Dispose();
            return response;
        }

        public  Response InsertOperationCombinationNotAllowed(ApplicationPMX application, Operation operationA, Operation operationB, DateTime declineDate, User registerUser)
        {
            var response = new Response { Message = "Not Initialized", Result = false };

            if (operationA.OperationId == operationB.OperationId)
            {
                response.Message = "La combinación no puede realizarse con la misma operación";
                response.Result = false;
                return response;
            }

            var resultExist = OperationsNotAllowedCombinationExist(application, operationA, operationB);
            if (resultExist.Result)
            {
                response.Message = string.Format("No se puede insertar la combinación.{0}", resultExist.Message);
                response.Result = false;
                return response;
            }


            var operationDA = new OperationDA(_configuration);
            try
            {
                operationDA.InsertOperationNotAllowedCombination(application, operationA, operationB, declineDate, registerUser);
                response.Result = true;
                response.Message =
                            string.Format(
                                "Se insertó la combinación no permitida de la operación {0} y {1} de la aplicación {2}.", operationA.OperationName, operationB.OperationName, application.ApplicationName);

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
                var resultLog = loglogic.InsertLogEvent(log);
                loglogic.Dispose();

            }
            catch (Exception e)
            {

                response.Message =
                    string.Format(
                        "Ocurrio un error al insertar la combinación de operaciones. {0}",
                        e.Message);
                response.Result = false;
            }
            operationDA.Dispose();
            return response;
        }

        public  Response UpdateOperationCombinationNotAllowed(ApplicationPMX application, Operation operationA, Operation operationB, DateTime declineDate, User registerUser)
        {
            //Se debe de regresar falso si no existe y se debe de interpretar en la llamada de la funcion
            var response = new Response { Message = "No existe la combinacion", Result = false };
            var operationDA = new OperationDA(_configuration);
            try
            {
                operationDA.UpdateOperationNotAllowedCombination(application, operationA, operationB, declineDate, registerUser);

               #region logRegister
               var log = new Log
               {
                   Application = new ApplicationPMX
                   {
                       ApplicationName = _applicationName
                   },
                   EventUser = registerUser,
                   EventTypeId = LogTypeEnum.Notification,
                   LogDescription = string.Format("Se actualizó la fecha de vigencia de combinación no autorizada de operaciones. Operaciones {0} - {1} nueva fecha de vigencia {3}", operationA.OperationId.ToString() + " " + operationA.OperationName, operationB.OperationId.ToString() + " " + operationB.OperationName,declineDate.ToShortDateString())
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
                        "Ocurrio un error al eliminar la combinacion de operaciones. {0}",
                        e.Message);
                response.Result = false;
            }
            response.Result = true;
            response.Message =
                        string.Format(
                            "Se eliminó la combinación no permitida de la operacion {0} y {1} de la aplicacion {2}.", operationA.OperationName, operationB.OperationName, application.ApplicationName);
            operationDA.Dispose();
            return response;

        }

        public  Response OperationsNotAllowedCombinationExist(ApplicationPMX application, Operation operationA, Operation operationB)
        {
            //Se debe de regresar falso si no existe y se debe de interpretar en la llamada de la funcion
            var response = new Response { Message = "No existe la combinacion", Result = true };
            var operationDa = new OperationDA(_configuration);
            try
            {
                response.Result = operationDa.OperationNotAllowedCombinationExist(application,
                                                                                                    operationA,
                                                                                                    operationB);
                if (response.Result)
                {
                    response.Message =
                        string.Format(
                            "La combinación no permitida de la operacion {0} y {1} de la aplicacion {2}, se encuentra registrada.", operationA.OperationName, operationB.OperationName, application.ApplicationName);

                }
                else
                {
                    response.Message =
                        string.Format(
                            "La combinación no permitida de la operacion {0} y {1} de la aplicacion {2}, no existe registrada.", operationA.OperationName, operationB.OperationName, application.ApplicationName);
                }
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
            operationDa.Dispose();
            return response;
        }

        public  Response OperationsNotAllowedCombinationExistAndDate(ApplicationPMX application, Operation operationA, Operation operationB)
        {
            //Se debe de regresar falso si no existe y se debe de interpretar en la llamada de la funcion
            var response = new Response { Message = "No existe la combinacion", Result = true };
            var operationDa = new OperationDA(_configuration);
            try
            {
                response.Result = operationDa.OperationNotAllowedCombinationExistAndDate(application,
                                                                                                    operationA,
                                                                                                    operationB);
                if (response.Result)
                {
                    response.Message =
                        string.Format(
                            "La combinación no permitida de la operacion {0} y {1} de la aplicacion {2}, se encuentra registrada.", operationA.OperationName, operationB.OperationName, application.ApplicationName);

                }
                
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
            operationDa.Dispose();
            return response;
        }


        public void Dispose()
        {
           GC.SuppressFinalize(this);
        }
    }
}
