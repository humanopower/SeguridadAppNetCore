using DataAccessContracts;
using EntityLibrary;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DataAccessSecurity
{
	public class OperationDA : IOperationDA
	{
		#region Propiedades

		private readonly IConfiguration _configuration;
		private string StrConexion { get; set; }

		#endregion Propiedades

		#region Metodos Publicos

		public OperationDA(IConfiguration configuration)
		{
			_configuration = configuration;
			StrConexion = _configuration.GetConnectionString("SecurityConnectionString");
		}

		public void AddOperation(Operation operation, User registerUser)
		{
			var conn = StrConexion;

			using (var sqlCon = new SqlConnection(conn))
			{
				sqlCon.Open();
				var cmd = new SqlCommand
				{
					Connection = sqlCon,
					CommandType = CommandType.StoredProcedure,
					CommandText = "usp_OperationsInsert"
				};
				cmd.Parameters.AddWithValue("@OperationName", operation.OperationName);
				cmd.Parameters.AddWithValue("@OperationDescription", operation.OperationDescription);
				cmd.Parameters.AddWithValue("@ApplicationId", operation.ApplicationId);
				cmd.Parameters.AddWithValue("@CreationUserId", registerUser.UserId);
				cmd.Parameters.AddWithValue("@ModificationUserId", registerUser.UserId);
				cmd.Parameters.AddWithValue("@DeclineDate", Convert.ToDateTime(operation.DeclineDate));

				cmd.Parameters.Add("@OperationId", SqlDbType.Int).Direction = ParameterDirection.Output;

				cmd.ExecuteNonQuery();
				operation.OperationId = (int)cmd.Parameters["@OperationId"].Value;
			}
		}

		public void UpdOperation(Operation operation, User registerUser)
		{
			var conn = StrConexion;

			using (var sqlCon = new SqlConnection(conn))
			{
				sqlCon.Open();
				var cmd = new SqlCommand
				{
					Connection = sqlCon,
					CommandType = CommandType.StoredProcedure,
					CommandText = "usp_OperationsUpdate"
				};

				cmd.Parameters.AddWithValue("@OperationId", operation.OperationId);
				cmd.Parameters.AddWithValue("@OperationName", operation.OperationName);
				cmd.Parameters.AddWithValue("@OperationDescription", operation.OperationDescription);
				cmd.Parameters.AddWithValue("@ApplicationId", operation.ApplicationId);
				cmd.Parameters.AddWithValue("@ModificationUserId", registerUser.UserId);
				cmd.Parameters.AddWithValue("@DeclineDate", Convert.ToDateTime(operation.DeclineDate));
				cmd.ExecuteNonQuery();
			}
		}

		public void DelOperation(Operation operation, User registerUser)
		{
			var conn = StrConexion;

			using (var sqlCon = new SqlConnection(conn))
			{
				sqlCon.Open();
				var cmd = new SqlCommand
				{
					Connection = sqlCon,
					CommandType = CommandType.StoredProcedure,
					CommandText = "usp_OperationsDelete"
				};

				cmd.Parameters.AddWithValue("@OperationId", operation.OperationId);
				cmd.Parameters.AddWithValue("@ApplicationId", operation.ApplicationId);
				cmd.Parameters.AddWithValue("@ModificationUserId", registerUser.UserId);
				cmd.Parameters.AddWithValue("@DeclineDate", Convert.ToDateTime(string.Format("{0:yyyy/MM/dd}", Convert.ToDateTime(operation.DeclineDate))));
				cmd.ExecuteNonQuery();
			}
		}

		public List<Operation> GetAllOperations()
		{
			var conn = StrConexion;
			var operationList = new List<Operation>();
			using (var sqlCon = new SqlConnection(conn))
			{
				sqlCon.Open();
				var cmd = new SqlCommand
				{
					Connection = sqlCon,
					CommandType = CommandType.StoredProcedure,
					CommandText = "usp_GetAllOperations"
				};

				SqlDataReader reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					var operation = new Operation
					{
						OperationId = (int)reader["OperationId"],
						OperationName = (string)reader["OperationName"],
						OperationDescription = (string)reader["OperationDescription"]
					};
					operationList.Add(operation);
				}
			}
			return operationList;
		}

		/// <summary>
		/// Obtiene las operaciones de un grupo de roles
		/// </summary>
		/// <param name="roleList"></param>
		/// <returns></returns>
		public List<Operation> GetOperationsList(List<Role> roleList)
		{
			var conn = StrConexion;
			var operationList = new List<Operation>();
			using (var sqlCon = new SqlConnection(conn))
			{
				sqlCon.Open();
				var cmd = new SqlCommand
				{
					Connection = sqlCon,
					CommandType = CommandType.StoredProcedure,
					CommandText = "usp_RoleOperationSelectByRoleId"
				};

				var dt = new DataTable();
				dt.Columns.Add(new DataColumn("RoleId"));
				var intRoleList = new List<int>();
				foreach (var role in roleList)
				{
					intRoleList.Add(role.RoleId);
					dt.Rows.Add(role.RoleId);
				}

				SqlParameter parameter = cmd.Parameters.AddWithValue("@RoleIds", dt);
				parameter.SqlDbType = SqlDbType.Structured;
				SqlDataReader reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					var operation = new Operation
					{
						OperationId = (int)reader["OperationId"]
						,
						OperationName = (string)reader["OperationName"]
						,
						OperationDescription = (string)reader["OperationDescription"]
					};
					operationList.Add(operation);
				}
			}
			return operationList;
		}

		/// <summary>
		/// Obtiene lista de operaciones a partir de un rol especifico.
		/// </summary>
		/// <param name="role">Objeto tipo rol</param>
		/// <returns>Lista de operaciones</returns>
		public List<Operation> GetRoleOperations(Role role)
		{
			var conn = StrConexion;
			var operationlst = new List<Operation>();
			using (var sqlCon = new SqlConnection(conn))
			{
				sqlCon.Open();
				var cmd = new SqlCommand
				{
					Connection = sqlCon,
					CommandType = CommandType.StoredProcedure,
					CommandText = "usp_GetoperationRole"
				};
				cmd.Parameters.AddWithValue("@RoleId", role.RoleId);

				var reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					var objOperation = new Operation
					{
						OperationId = (int)reader["OperationId"],
						OperationName = reader["OperationName"].ToString(),
						OperationDescription = reader["OperationDescription"].ToString(),
						CreationDateTime = (DateTime)reader["CreationDateTime"],
						CreationUserId = reader["CreationUserId"].ToString(),
						ModificationUserId = string.IsNullOrEmpty(reader["ModificationUserId"].ToString()) ? string.Empty : reader["ModificationUserId"].ToString(),
						DeclineDate = string.IsNullOrEmpty(reader["DeclineDate"].ToString()) ? string.Empty : string.Format("{0:dd/MM/yyyy}", reader["DeclineDate"])
					};
					operationlst.Add(objOperation);
				}
			}

			return operationlst;
		}

		public List<Operation> GetOperationsForItems(string strValue)
		{
			var conn = StrConexion;
			var operationList = new List<Operation>();
			using (var sqlCon = new SqlConnection(conn))
			{
				sqlCon.Open();
				var cmd = new SqlCommand
				{
					Connection = sqlCon,
					CommandType = CommandType.StoredProcedure,
					CommandText = "usp_FindOperationsByItems"
				};

				cmd.Parameters.AddWithValue("@strCampo", strValue);

				SqlDataReader reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					var operation = new Operation
					{
						OperationId = (int)reader["OperationId"],
						OperationName = (string)reader["OperationName"],
						OperationDescription = (string)reader["OperationDescription"],
						ApplicationId = (int)reader["ApplicationId"]
					};
					operationList.Add(operation);
				}
			}
			return operationList;
		}

		public Operation GetDataByIdOperation(int operationId)
		{
			var conn = StrConexion;
			var objOperation = new Operation();
			using (var sqlCon = new SqlConnection(conn))
			{
				sqlCon.Open();
				var cmd = new SqlCommand
				{
					Connection = sqlCon,
					CommandType = CommandType.StoredProcedure,
					CommandText = "usp_FindOperationsById"
				};

				cmd.Parameters.AddWithValue("@IdOperation", operationId);

				SqlDataReader reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					var operation = new Operation
					{
						OperationId = (int)reader["OperationId"],
						OperationName = (string)reader["OperationName"],
						OperationDescription = (string)reader["OperationDescription"],
						ApplicationId = (int)reader["ApplicationId"],
						DeclineDate = Convert.ToDateTime(reader["DeclineDate"]).ToShortDateString()
					};
					objOperation = operation;
				}
			}
			return objOperation;
		}

		public Operation GetDataByOperationName(Operation operation)
		{
			var conn = StrConexion;
			Operation objoperation = null;
			using (var sqlCon = new SqlConnection(conn))
			{
				sqlCon.Open();
				var cmd = new SqlCommand
				{
					Connection = sqlCon,
					CommandType = CommandType.StoredProcedure,
					CommandText = "usp_FindOperationsByName"
				};

				cmd.Parameters.AddWithValue("@strCampo", operation.OperationName);
				cmd.Parameters.AddWithValue("@ApplicationId", operation.ApplicationId);

				SqlDataReader reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					var objOperation = new Operation
					{
						OperationId = (int)reader["OperationId"],
						OperationName = (string)reader["OperationName"],
						OperationDescription = (string)reader["OperationDescription"],
						ApplicationId = (int)reader["ApplicationId"]
					};

					objoperation = objOperation;
				}
			}

			return objoperation;
		}

		public List<Operation> GetOperationsList(ApplicationPMX application)
		{
			var conn = StrConexion;
			var operationlst = new List<Operation>();
			using (var sqlCon = new SqlConnection(conn))
			{
				sqlCon.Open();
				var cmd = new SqlCommand
				{
					Connection = sqlCon,
					CommandType = CommandType.StoredProcedure,
					CommandText = "usp_OperationsSelectByApplicationId"
				};
				cmd.Parameters.AddWithValue("@ApplicationId", application.ApplicationId);

				var reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					var objOperation = new Operation
					{
						OperationId = (int)reader["OperationId"],
						OperationName = (string)reader["OperationName"],
						OperationDescription = (string)reader["OperationDescription"],
						ApplicationId = (int)reader["ApplicationId"],
						CreationDateTime = Convert.ToDateTime(string.Format("{0:dd/MM/yyyy}", reader["CreationDateTime"])),
						CreationUserId = reader["CreationUserId"].ToString(),
						ModificationDateTime = string.IsNullOrEmpty(reader["ModificationDateTime"].ToString()) ? string.Empty : string.Format("{0:dd/MM/yyyy}", reader["ModificationDateTime"]),
						ModificationUserId = reader["ModificationUserId"].ToString(),
						DeclineDate = string.IsNullOrEmpty(reader["DeclineDate"].ToString()) ? string.Empty : string.Format("{0:dd/MM/yyyy}", reader["DeclineDate"])
					};
					operationlst.Add(objOperation);
				}
			}
			return operationlst;
		}

		public void AddOperationToRole(Operation operation, Role role, User registerUser)
		{
			var conn = StrConexion;

			using (var sqlCon = new SqlConnection(conn))
			{
				sqlCon.Open();
				var cmd = new SqlCommand
				{
					Connection = sqlCon,
					CommandType = CommandType.StoredProcedure,
					CommandText = "usp_RoleOperationsInsert"
				};
				cmd.Parameters.AddWithValue("@RoleId", role.RoleId);
				cmd.Parameters.AddWithValue("@OperationId", operation.OperationId);
				cmd.Parameters.AddWithValue("@CreationUserId", registerUser.UserId);
				cmd.Parameters.AddWithValue("@ModificationUserId", registerUser.UserId);
				cmd.Parameters.AddWithValue("@DeclineDate", Convert.ToDateTime(role.DeclineDate));
				cmd.ExecuteNonQuery();
			}
		}

		public void DeleteOperationToRole(Operation operation, Role role)
		{
			var conn = StrConexion;

			using (var sqlCon = new SqlConnection(conn))
			{
				sqlCon.Open();
				var cmd = new SqlCommand
				{
					Connection = sqlCon,
					CommandType = CommandType.StoredProcedure,
					CommandText = "usp_RoleOperationsDelete"
				};
				cmd.Parameters.AddWithValue("@RoleId", role.RoleId);
				cmd.Parameters.AddWithValue("@OperationId", operation.OperationId);

				cmd.ExecuteNonQuery();
			}
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}

		public bool OperationNotAllowedCombinationExist(ApplicationPMX application, Operation operationA, Operation operationB)
		{
			var conn = StrConexion;
			using (var sqlCon = new SqlConnection(conn))
			{
				sqlCon.Open();
				var cmd = new SqlCommand
				{
					Connection = sqlCon,
					CommandType = CommandType.StoredProcedure,
					CommandText = "usp_OperationsCombinationsNotAllowedSelectByApplicationIdAndOperations"
				};
				cmd.Parameters.AddWithValue("@ApplicationId", application.ApplicationId);
				cmd.Parameters.AddWithValue("@OperationIdA", operationA.OperationId);
				cmd.Parameters.AddWithValue("@OperationIdB", operationB.OperationId);
				var reader = cmd.ExecuteReader();
				if (reader.Read())
				{
					return true;
				}
			}
			return false;
		}

		public bool OperationNotAllowedCombinationExistAndDate(ApplicationPMX application, Operation operationA, Operation operationB)
		{
			var conn = StrConexion;
			using (var sqlCon = new SqlConnection(conn))
			{
				sqlCon.Open();
				var cmd = new SqlCommand
				{
					Connection = sqlCon,
					CommandType = CommandType.StoredProcedure,
					CommandText = "usp_OperationsCombinationsNotAllowedSelectByApplicationIdAndOperationsAndDate"
				};
				cmd.Parameters.AddWithValue("@ApplicationId", application.ApplicationId);
				cmd.Parameters.AddWithValue("@OperationIdA", operationA.OperationId);
				cmd.Parameters.AddWithValue("@OperationIdB", operationB.OperationId);
				var reader = cmd.ExecuteReader();
				if (reader.Read())
				{
					return true;
				}
			}
			return false;
		}

		public void UpdateOperationNotAllowedCombination(ApplicationPMX application, Operation operationA, Operation operationB, DateTime declineDate, User registerUser)
		{
			var conn = StrConexion;
			using (var sqlCon = new SqlConnection(conn))
			{
				sqlCon.Open();
				var cmd = new SqlCommand
				{
					Connection = sqlCon,
					CommandType = CommandType.StoredProcedure,
					CommandText = "usp_OperationsCombinationsNotAllowedUpdate"
				};
				cmd.Parameters.AddWithValue("@ApplicationId", application.ApplicationId);
				cmd.Parameters.AddWithValue("@OperationIdA", operationA.OperationId);
				cmd.Parameters.AddWithValue("@OperationIdB", operationB.OperationId);
				cmd.Parameters.AddWithValue("@UserId", registerUser.UserId);
				cmd.Parameters.AddWithValue("@DeclineDate", declineDate);
				cmd.ExecuteNonQuery();
			}
		}

		public void InsertOperationNotAllowedCombination(ApplicationPMX application, Operation operationA, Operation operationB, DateTime declineDate, User registerUser)
		{
			var conn = StrConexion;
			using (var sqlCon = new SqlConnection(conn))
			{
				sqlCon.Open();
				var cmd = new SqlCommand
				{
					Connection = sqlCon,
					CommandType = CommandType.StoredProcedure,
					CommandText = "usp_OperationsCombinationsNotAllowedInsert"
				};
				cmd.Parameters.AddWithValue("@ApplicationId", application.ApplicationId);
				cmd.Parameters.AddWithValue("@OperationIdA", operationA.OperationId);
				cmd.Parameters.AddWithValue("@OperationIdB", operationB.OperationId);
				cmd.Parameters.AddWithValue("@UserId", registerUser.UserId);
				cmd.Parameters.AddWithValue("@DeclineDate", declineDate);
				cmd.ExecuteNonQuery();
			}
		}

		public DataTable GetOperationsCombinationsNotAllowed(ApplicationPMX application)
		{
			var conn = StrConexion;
			DataTable dt = new DataTable();
			using (var sqlCon = new SqlConnection(conn))
			{
				sqlCon.Open();
				var cmd = new SqlCommand
				{
					Connection = sqlCon,
					CommandType = CommandType.StoredProcedure,
					CommandText = "usp_OperationsCombinationsNotAllowedSelectByApplicationId"
				};
				cmd.Parameters.AddWithValue("@ApplicationId", application.ApplicationId);
				SqlDataReader dr = cmd.ExecuteReader();
				dt.Load(dr);
			}
			return dt;
		}

		#endregion Metodos Publicos
	}
}