using DataAccessContracts;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;

namespace DataAccessSecurity
{
	public class ResponsiveDA : IResponsiveDA
	{
		private readonly IConfiguration _configuration;
		private string StrConexion { get; set; }

		public ResponsiveDA(IConfiguration configuration)
		{
			_configuration = configuration;
			StrConexion = _configuration.GetConnectionString("SecurityConnectionString");
		}

		public void AddUserResponsive(string userId, int applicationId, int roleId, string userValidator)
		{
			string conn = StrConexion;
			using (var sqlCon = new SqlConnection(conn))
			{
				sqlCon.Open();
				var cmd = new SqlCommand
				{
					Connection = sqlCon,
					CommandType = CommandType.StoredProcedure,
					CommandText = "usp_UserApplicationResponsiveInsert"
				};

				cmd.Parameters.AddWithValue("@ApplicationId", applicationId);
				cmd.Parameters.AddWithValue("@UserId", userId);
				cmd.Parameters.AddWithValue("@RoleId", roleId);
				cmd.Parameters.AddWithValue("@UserValidator", userValidator);
				cmd.ExecuteNonQuery();
			}
		}

		public void AuthorizeUserResponsive(int responsiveId)
		{
			string conn = StrConexion;
			using (var sqlCon = new SqlConnection(conn))
			{
				sqlCon.Open();
				var cmd = new SqlCommand
				{
					Connection = sqlCon,
					CommandType = CommandType.StoredProcedure,
					CommandText = "usp_UserApplicationResponsiveAuthorization"
				};

				cmd.Parameters.AddWithValue("@AcceptanceId", responsiveId);

				cmd.ExecuteNonQuery();
			}
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}
	}
}