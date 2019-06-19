using DataAccessContracts;
using EntityLibrary;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DataAccessSecurity
{
	public class RoleOperationDA : IRoleOperationDA
	{
		#region Propiedades

		private readonly IConfiguration _configuration;
		private string StrConexion { get; set; }

		#endregion Propiedades

		#region Constructor

		public RoleOperationDA(IConfiguration configuration)
		{
			_configuration = configuration;
			StrConexion = _configuration.GetConnectionString("SecurityConnectionString");
		}

		#endregion Constructor

		public int AddNewRoleOperation(RoleOperations roleOperations, User registerUser)
		{
			var conn = StrConexion;
			var iResultado = 0;
			using (var sqlCon = new SqlConnection(conn))
			{
				sqlCon.Open();
				var cmd = new SqlCommand
				{
					Connection = sqlCon,
					CommandType = CommandType.StoredProcedure,
					CommandText = "usp_RoleOperationsInsert"
				};

				cmd.Parameters.Add("@RoleId", SqlDbType.Int).Value = roleOperations.RoleId;
				cmd.Parameters.Add("@OperationId", SqlDbType.Int).Value = roleOperations.OperationId;
				cmd.Parameters.Add("@CreationUserId", SqlDbType.NVarChar).Value = registerUser.UserId;
				cmd.Parameters.Add("@DeclineDate", SqlDbType.DateTime).Value = Convert.ToDateTime(roleOperations.DeclineDate);

				iResultado = cmd.ExecuteNonQuery();
			}

			return iResultado;
		}

		public int DelRoleOperation(RoleOperations roleOperations, User registerUser)
		{
			var conn = StrConexion;
			var iResultado = 0;
			using (var sqlCon = new SqlConnection(conn))
			{
				sqlCon.Open();
				var cmd = new SqlCommand
				{
					Connection = sqlCon,
					CommandType = CommandType.StoredProcedure,
					CommandText = "usp_RoleOperationsDelete"
				};

				cmd.Parameters.Add("@RoleId", SqlDbType.Int).Value = roleOperations.RoleId;
				cmd.Parameters.Add("@OperationId", SqlDbType.Int).Value = roleOperations.OperationId;
				cmd.Parameters.Add("@ModificationUserId", SqlDbType.NVarChar).Value = registerUser.UserId;
				cmd.Parameters.Add("@DeclineDate", SqlDbType.DateTime).Value = Convert.ToDateTime(string.Format("{0:yyyy/MM/dd}", Convert.ToDateTime(roleOperations.DeclineDate)));

				iResultado = cmd.ExecuteNonQuery();
			}

			return iResultado;
		}

		public List<Operation> GetoperationRole(Role role)
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
					CommandText = "usp_GetoperationRole"
				};
				cmd.Parameters.AddWithValue("@RoleId", role.RoleId);

				SqlDataReader reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					var operation = new Operation()
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

		public List<RoleOperations> GetRoleOperations(Role role)
		{
			var conn = StrConexion;
			var roleOperationList = new List<RoleOperations>();
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

				SqlDataReader reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					var roleOperations = new RoleOperations()
					{
						RoleId = (int)reader["RoleId"],
						OperationId = (int)reader["OperationId"]
					};

					roleOperationList.Add(roleOperations);
				}
			}

			return roleOperationList;
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}
	}
}