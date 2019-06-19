using DataAccessContracts;
using EntityLibrary;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DataAccessSecurity
{
	public class ApplicationDA : IApplicationDA

	{
		#region Atributos

		private IConfiguration configuration;
		private SqlConnection _cnn = null;
		private SqlCommand _cmd = null;
		private SqlDataAdapter _da = null;
		private DataSet _ds = null;

		#endregion Atributos

		#region Propiedades

		private readonly IConfiguration _configuration;
		private string StrConexion { get; set; }

		#endregion Propiedades

		#region Constructor

		public ApplicationDA(IConfiguration configuration)
		{
			_configuration = configuration;
			StrConexion = _configuration.GetConnectionString("SecurityConnectionString");
		}

		#endregion Constructor

		#region Metodos Publicos

		public void AddApplication(ApplicationPMX application, User registerUser)
		{
			string conn = string.Empty;
			using (var sqlCon = new SqlConnection(conn))
			{
				sqlCon.Open();
				var cmd = new SqlCommand
				{
					Connection = sqlCon,
					CommandType = CommandType.StoredProcedure,
					CommandText = "usp_ApplicationsInsert"
				};

				cmd.Parameters.AddWithValue("@ApplicationName", application.ApplicationName);
				cmd.Parameters.AddWithValue("@ApplicationDescription", application.ApplicationDescription);
				cmd.Parameters.AddWithValue("@ValidityStartDate",
					 Convert.ToDateTime(string.Format("{0:yyyy/MM/dd}", Convert.ToDateTime(application.ValidityStartDate))));
				cmd.Parameters.AddWithValue("@Observations", application.Observations);
				cmd.Parameters.AddWithValue("@TecnicalUserId", application.TecnicalUserId);
				cmd.Parameters.AddWithValue("@TecnicalUserIdDos", application.TecnicalUserIdDos);
				cmd.Parameters.AddWithValue("@TecnicalUserIdTres", application.TecnicalUserIdTres);
				cmd.Parameters.AddWithValue("@TecnicalUserIdCuatro", application.TecnicalUserIdCuatro);
				cmd.Parameters.AddWithValue("@FunctionalUserId", application.FunctionalUserId);
				cmd.Parameters.AddWithValue("@CreationUserId", registerUser.UserId);
				cmd.Parameters.AddWithValue("@DeclineDate",
					 Convert.ToDateTime(string.Format("{0:yyyy/MM/dd}", Convert.ToDateTime(application.DeclineDate))));
				cmd.Parameters.AddWithValue("@HashedPassword", application.ApplicationPassword);

				cmd.Parameters.Add("@IdApplication", SqlDbType.Int).Direction = ParameterDirection.Output;

				cmd.ExecuteNonQuery();

				//SqlParameter idAppOut = cmd.Parameters.Add("@IdApplication", SqlDbType.Int);
				//idAppOut.Direction = ParameterDirection.Output;
				//application.ApplicationId =(int)  idAppOut.Value;

				application.ApplicationId = (int)cmd.Parameters["@IdApplication"].Value;
			}
		}

		public void DelApplication(ApplicationPMX application, User registerUser)
		{
			var conn = StrConexion;
			using (var sqlCon = new SqlConnection(conn))
			{
				sqlCon.Open();
				var cmd = new SqlCommand
				{
					Connection = sqlCon,
					CommandType = CommandType.StoredProcedure,
					CommandText = "usp_ApplicationsDelete"
				};
				cmd.Parameters.AddWithValue("@ApplicationId", application.ApplicationId);
				cmd.Parameters.AddWithValue("@DeclinateDate",
					 Convert.ToDateTime(string.Format("{0:yyyy/MM/dd}", Convert.ToDateTime(application.DeclineDate))));
				cmd.Parameters.AddWithValue("@ModificationUserId", registerUser.UserId);
				cmd.ExecuteNonQuery();
			}
		}

		public void UpdApplication(ApplicationPMX application, User registerUser)
		{
			var conn = StrConexion;
			using (var sqlCon = new SqlConnection(conn))
			{
				sqlCon.Open();
				var cmd = new SqlCommand
				{
					Connection = sqlCon,
					CommandType = CommandType.StoredProcedure,
					CommandText = "usp_ApplicationsUpdate"
				};

				cmd.Parameters.AddWithValue("@ApplicationId", application.ApplicationId);
				cmd.Parameters.AddWithValue("@ApplicationName", application.ApplicationName);
				cmd.Parameters.AddWithValue("@ApplicationDescription", application.ApplicationDescription);
				cmd.Parameters.AddWithValue("@ValidityStartDate",
					 Convert.ToDateTime(string.Format("{0:yyyy/MM/dd}", Convert.ToDateTime(application.ValidityStartDate))));
				cmd.Parameters.AddWithValue("@DeclineDate",
					 Convert.ToDateTime(string.Format("{0:yyyy/MM/dd}", Convert.ToDateTime(application.DeclineDate))));
				cmd.Parameters.AddWithValue("@Observations", application.Observations);
				cmd.Parameters.AddWithValue("@TecnicalUserId", application.TecnicalUserId);
				cmd.Parameters.AddWithValue("@TecnicalUserIdDos", application.TecnicalUserIdDos);
				cmd.Parameters.AddWithValue("@TecnicalUserIdTres", application.TecnicalUserIdTres);
				cmd.Parameters.AddWithValue("@TecnicalUserIdCuatro", application.TecnicalUserIdCuatro);
				cmd.Parameters.AddWithValue("@FunctionalUserId", application.FunctionalUserId);
				cmd.Parameters.AddWithValue("@ModificationUserId", registerUser.UserId);
				cmd.Parameters.AddWithValue("@HashedPassword", application.ApplicationPassword);

				cmd.ExecuteNonQuery();
			}
		}

		public List<ApplicationPMX> GetApplicationList()
		{
			var conn = StrConexion;
			var appList = new List<ApplicationPMX>();
			using (var sqlCon = new SqlConnection(conn))
			{
				sqlCon.Open();
				var cmd = new SqlCommand
				{
					Connection = sqlCon,
					CommandType = CommandType.StoredProcedure,
					CommandText = "usp_ApplicationsSelect"
				};

				SqlDataReader reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					var app = new ApplicationPMX
					{
						ApplicationDescription = (string)reader["ApplicationDescription"]
						 ,
						ApplicationId = (int)reader["ApplicationId"]
						 ,
						ApplicationName = (string)reader["ApplicationName"]
						 ,
						ApplicationPassword = (string)reader["HashedPassword"]
						 ,
						DeclineDate = ((DateTime)reader["DeclineDate"]).ToShortDateString()
						 ,
						DeclineDateDF = (DateTime)reader["DeclineDate"]
						 ,
						ValidityStartDate = ((DateTime)reader["ValidityStartDate"]).ToShortDateString()
					};
					appList.Add(app);
				}
			}
			return appList;
		}

		public List<ApplicationPMX> GetApplicationList(string strValue)
		{
			var conn = StrConexion;
			var appList = new List<ApplicationPMX>();
			using (var sqlCon = new SqlConnection(conn))
			{
				sqlCon.Open();
				var cmd = new SqlCommand
				{
					Connection = sqlCon,
					CommandType = CommandType.StoredProcedure,
					CommandText = "usp_GetApplications"
				};

				cmd.Parameters.AddWithValue("@strCampo", strValue);
				SqlDataReader reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					var app = new ApplicationPMX
					{
						ApplicationId = (int)reader["ApplicationId"],
						ApplicationName = (string)reader["ApplicationName"],
						ApplicationDescription = (string)reader["ApplicationDescription"],
						ApplicationPassword = (string)reader["Observations"]
					};
					appList.Add(app);
				}
			}
			return appList;
		}

		/// <summary>
		/// Metodo que busca información en lo siguientes campos: ApplicationId, ApplicationName, ApplicationDescription, Observations
		/// </summary>
		/// <param name="strValue">Valor del campo a buscar</param>
		/// <returns>Regresa un objeto de Tipo DataSet</returns>
		public DataSet SearchApplication(string strValue, ApplicationPMX application, User loggedUser)
		{
			ConnectDb();

			_cmd = new SqlCommand();
			_da = new SqlDataAdapter();
			_ds = new DataSet();

			_cmd.Connection = _cnn;
			_cmd.CommandType = CommandType.StoredProcedure;
			_cmd.CommandText = "usp_FindApplicationsConAmbito";
			_cmd.Parameters.AddWithValue("@strCampo", strValue);
			_cmd.Parameters.AddWithValue("@UserId", application.FunctionalUserId);
			_cmd.Parameters.AddWithValue("@LoggedUserId", loggedUser.UserId);
			_cmd.CommandTimeout = 0;

			_da.SelectCommand = _cmd;
			_da.Fill(_ds);

			DisconnectDb();

			return _ds;
		}

		/// <summary>
		/// Metodo que obtiene las aplicaciones por Usuario
		/// </summary>
		/// <param name="strUser">Id del Usuario</param>
		/// <returns>Regresa un objeto de tipo DataSet</returns>
		public DataSet FindApplicationforUser(string strUser)
		{
			ConnectDb();

			_cmd = new SqlCommand();
			_da = new SqlDataAdapter();
			_ds = new DataSet();

			_cmd.Connection = _cnn;
			_cmd.CommandType = CommandType.StoredProcedure;
			_cmd.CommandText = "usp_FindApplicationsforUser";
			_cmd.Parameters.AddWithValue("@strUser", strUser);
			_cmd.CommandTimeout = 0;

			_da.SelectCommand = _cmd;
			_da.Fill(_ds);

			DisconnectDb();

			return _ds;
		}

		public ApplicationPMX FindApplication(int idApplication)
		{
			var conn = StrConexion;
			var objApp = new ApplicationPMX();
			using (var sqlCon = new SqlConnection(conn))
			{
				sqlCon.Open();
				var cmd = new SqlCommand
				{
					Connection = sqlCon,
					CommandType = CommandType.StoredProcedure,
					CommandText = "usp_FindApplication"
				};

				cmd.Parameters.AddWithValue("@IdApplication", idApplication);

				SqlDataReader reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					var app = new ApplicationPMX();

					app.ApplicationDescription = (string)reader["ApplicationDescription"];
					app.ApplicationId = (int)reader["ApplicationId"];
					app.ApplicationName = (string)reader["ApplicationName"];
					app.ValidityStartDate = string.IsNullOrEmpty(reader["ValidityStartDate"].ToString())
						 ? string.Empty
						 : string.Format("{0:dd/MM/yyyy}", reader["ValidityStartDate"]);
					app.DeclineDate = string.IsNullOrEmpty(reader["DeclineDate"].ToString())
						 ? string.Empty
						 : string.Format("{0:dd/MM/yyyy}", reader["DeclineDate"]);
					app.Observations = (string)reader["Observations"];
					app.ApplicationPassword = (string)reader["HashedPassword"];
					app.FunctionalUserId = (string)reader["FunctionalUserId"];
					app.TecnicalUserId = (string)reader["TecnicalUserId"];
					app.TecnicalUserIdDos = reader["TecnicalUserIdDos"] == System.DBNull.Value
						 ? string.Empty
						 : (string)reader["TecnicalUserIdDos"];
					app.TecnicalUserIdTres = reader["TecnicalUserIdTres"] == System.DBNull.Value
						 ? string.Empty
						 : (string)reader["TecnicalUserIdTres"];
					app.TecnicalUserIdCuatro = reader["TecnicalUserIdCuatro"] == System.DBNull.Value
						 ? string.Empty
						 : (string)reader["TecnicalUserIdCuatro"];

					objApp = app;
				}

				return objApp;
			}
		}

		public Response AddApplicationAdministration(ApplicationPMX application, User user,
			 bool canAdminAppRolesAndOperations, bool canAdminUsers, string userId)
		{
			var response = new Response() { Message = "Not initizalizated", Result = false };
			string userIdRecuperado = string.Empty;
			if (!(string.IsNullOrEmpty(userId)))
			{
				string[] usuario = userId.Split(new Char[] { ' ' });
				userIdRecuperado = usuario[0];
			}

			try
			{
				var conn = StrConexion;

				using (var sqlCon = new SqlConnection(conn))
				{
					sqlCon.Open();

					var cmd = new SqlCommand
					{
						Connection = sqlCon,
						CommandType = CommandType.StoredProcedure,
						CommandText = "usp_ApplicationAdministrationInsert"
					};

					cmd.Parameters.AddWithValue("@ApplicationId", application.ApplicationId);
					cmd.Parameters.AddWithValue("@UserId", userIdRecuperado);
					cmd.Parameters.AddWithValue("@CanAdminApplicationRO", canAdminAppRolesAndOperations);
					cmd.Parameters.AddWithValue("@CanAdminApplicationUsers", canAdminUsers);
					cmd.ExecuteNonQuery();
				}
				response.Message = string.Format(
					 "Se agregó correctamente al usuario {0} para administrar la aplicacion {1}", user.UserId,
					 application.ApplicationName);
				response.Result = true;
			}
			catch (Exception exception)
			{
				response.Result = false;
				response.Message = String.Format("Ocurrió un error al agregar al usuario. {0}", exception.Message);
			}

			return response;
		}

		public Response UpdApplicationAdministration(ApplicationPMX application, bool canAdminAppRolesAndOperations,
			 bool canAdminUsers, string strUser)
		{
			var response = new Response() { Message = "Not initizalizated", Result = false };
			string[] strUserRecuperado = strUser.Split(new Char[] { ' ' });
			try
			{
				var conn = StrConexion;

				using (var sqlCon = new SqlConnection(conn))
				{
					sqlCon.Open();

					var cmd = new SqlCommand
					{
						Connection = sqlCon,
						CommandType = CommandType.StoredProcedure,
						CommandText = "usp_ApplicationAdministrationUpdate"
					};

					cmd.Parameters.AddWithValue("@ApplicationId", application.ApplicationId);
					cmd.Parameters.AddWithValue("@UserId", strUserRecuperado[0]);
					cmd.Parameters.AddWithValue("@CanAdminApplicationRO", canAdminAppRolesAndOperations);
					cmd.Parameters.AddWithValue("@CanAdminApplicationUsers", canAdminUsers);
					cmd.ExecuteNonQuery();
				}
				response.Message =
					 string.Format("Se actualizó correctamente el o los usuarios para administrar la aplicacion {0}",
						  application.ApplicationName);
				response.Result = true;
			}
			catch (Exception exception)
			{
				response.Result = false;
				response.Message = String.Format("Ocurrió un error al agregar al usuario. {0}", exception.Message);
			}

			return response;
		}

		public Response DelApplicationAdministration(ApplicationPMX application, User user)
		{
			var response = new Response() { Message = "Not initizalizated", Result = false };
			try
			{
				var conn = StrConexion;
				string[] usuario = user.UserId.Split(' ');
				using (var sqlCon = new SqlConnection(conn))
				{
					sqlCon.Open();
					var cmd = new SqlCommand
					{
						Connection = sqlCon,
						CommandType = CommandType.StoredProcedure,
						CommandText = "usp_ApplicationAdministrationDelete"
					};

					cmd.Parameters.AddWithValue("@ApplicationId", application.ApplicationId);
					cmd.Parameters.AddWithValue("@UserId", usuario[0]);
					cmd.ExecuteNonQuery();
				}
				response.Message =
					 string.Format(
						  "Se elimino correctamente la clave del usuario funcional  {0} para administrar la aplicacion {1}",
						  user.UserId, application.ApplicationName);
				response.Result = true;
			}
			catch (Exception exception)
			{
				response.Result = false;
				response.Message = String.Format("Ocurrió un error al agregar al usuario. {0}", exception.Message);
			}

			return response;
		}

		public object[] GetApplicationAdministration(ApplicationPMX application)
		{
			var response = new Response() { Message = "Not initizalizated", Result = false };
			var ds = new DataSet();
			object[] obj = null;
			try
			{
				var conn = StrConexion;

				using (var sqlCon = new SqlConnection(conn))
				{
					sqlCon.Open();
					var da = new SqlDataAdapter();

					var cmd = new SqlCommand
					{
						Connection = sqlCon,
						CommandType = CommandType.StoredProcedure,
						CommandText = "usp_ApplicationAdministrationSelect"
					};

					cmd.Parameters.AddWithValue("@ApplicationId", application.ApplicationId);
					cmd.Parameters.AddWithValue("@UserId", application.FunctionalUserId);

					da.SelectCommand = cmd;

					da.Fill(ds);

					obj = new object[ds.Tables[0].Rows.Count];

					ds.Tables[0].Rows.CopyTo(obj, 0);
				}
			}
			catch (Exception exception)
			{
				response.Result = false;
				response.Message = String.Format("Ocurrió un error al agregar al usuario. {0}", exception.Message);
			}

			return obj;
		}

		public DataSet SearchCuentasRolesData(int idAplicacion)
		{
			ConnectDb();

			_cmd = new SqlCommand();
			_da = new SqlDataAdapter();
			_ds = new DataSet();

			_cmd.Connection = _cnn;
			_cmd.CommandType = CommandType.StoredProcedure;
			_cmd.CommandText = "usp_GetCuentasRolesAsignados";
			_cmd.Parameters.AddWithValue("@aplicationId", idAplicacion);
			_cmd.CommandTimeout = 0;

			_da.SelectCommand = _cmd;
			_da.Fill(_ds);

			DisconnectDb();

			return _ds;
		}

		public DataSet SearchCuentasSistemaRol(int rolId)
		{
			ConnectDb();

			_cmd = new SqlCommand();
			_da = new SqlDataAdapter();
			_ds = new DataSet();

			_cmd.Connection = _cnn;
			_cmd.CommandType = CommandType.StoredProcedure;
			_cmd.CommandText = "usp_GetCuentasSistemaRol";
			_cmd.Parameters.AddWithValue("@rolId", rolId);
			_cmd.CommandTimeout = 0;

			_da.SelectCommand = _cmd;
			_da.Fill(_ds);

			DisconnectDb();

			return _ds;
		}

		#endregion Metodos Publicos

		#region Metodos Privados

		/// <summary>
		/// Metodo que crea el objeto conexion para enlazar con la base de datos
		/// </summary>
		private void ConnectDb()
		{
			_cnn = new SqlConnection { ConnectionString = StrConexion };
			_cnn.Open();
		}

		/// <summary>
		/// Metodo que cierra el objeto conexion
		/// </summary>
		private void DisconnectDb()
		{
			if (_cnn.State == ConnectionState.Open)
			{
				_cnn.Close();
			}
		}

		#endregion Metodos Privados

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}
	}
}