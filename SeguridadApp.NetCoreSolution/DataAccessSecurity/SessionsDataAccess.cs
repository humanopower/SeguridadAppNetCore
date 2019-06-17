using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using DataAccessContracts;
using EntityLibrary;
using Microsoft.Extensions.Configuration;

namespace DataAccessSecurity
{
    public class SessionsDataAccess : ISessionsDataAccess
    {

		#region Propiedades
		private readonly IConfiguration _configuration;
		private string StrConexion { get; set; }

		#endregion

		#region Constructor

		public SessionsDataAccess(IConfiguration configuration)
		{
			_configuration = configuration;
			StrConexion = _configuration.GetConnectionString("SecurityConnectionString");
		}

		#endregion
		public User AddSession(User user, ApplicationPMX application)
        {
            var conn = StrConexion;
            
            using (var sqlCon = new SqlConnection(conn))
            {
                sqlCon.Open();
                var cmd = new SqlCommand
                {
                    Connection = sqlCon,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "usp_LoggingSessionsInsert"
                };

                cmd.Parameters.Add("@UserId", SqlDbType.NVarChar).Value = user.UserId;
                cmd.Parameters.Add("@ApplicationId", SqlDbType.NVarChar).Value = application.ApplicationId;
                //cmd.Parameters.Add("@CreationUserId", System.Data.SqlDbType.NVarChar).Value = registerUser.UserId;

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if(reader.Read())
                    {
                        user.SessionId = (Guid)reader["SessionID"];
                    }
                }
            }

            return user;
        }

        public  void FindSession(User user, ApplicationPMX application, out bool sessionFinded, out bool isSessionValid)
        {
            sessionFinded = false;
            isSessionValid = false;
            var conn = StrConexion;

            using (var sqlCon = new SqlConnection(conn))
            {
                sqlCon.Open();
                var cmd = new SqlCommand
                {
                    Connection = sqlCon,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "usp_LoggingSessionsSelect"
                };

                cmd.Parameters.Add("@SessionId", SqlDbType.UniqueIdentifier).Value = user.SessionId;
                cmd.Parameters.Add("@ApplicationId", SqlDbType.Int).Value = application.ApplicationId;
              
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read()) return;
                    sessionFinded = true;
                    isSessionValid = (bool) reader["IsSessionValid"];
                }
            }
        }

        public  void UpdateSessionEndTime(User user, ApplicationPMX application)
        {
           var conn = StrConexion;

            using (var sqlCon = new SqlConnection(conn))
            {
                sqlCon.Open();
                var cmd = new SqlCommand
                {
                    Connection = sqlCon,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "usp_LoggingSessionsUpdate"
                };

                cmd.Parameters.Add("@SessionId", SqlDbType.UniqueIdentifier).Value = user.SessionId;
                cmd.Parameters.Add("@ApplicationId", SqlDbType.Int).Value = application.ApplicationId;
                cmd.ExecuteNonQuery();
            }
        }


        public  string FindSessionUser(string sessionGuid)
        {
            var conn = StrConexion;
            string userId = null;
            using (var sqlCon = new SqlConnection(conn))
            {
                sqlCon.Open();
                var cmd = new SqlCommand
                {
                    Connection = sqlCon,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "usp_LoggingSessionsSelectBySession"
                };

                cmd.Parameters.Add("@SessionId", SqlDbType.UniqueIdentifier).Value = new Guid(sessionGuid);

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        userId = reader["UserId"].ToString();
                    }
                }
            }
            return userId;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
