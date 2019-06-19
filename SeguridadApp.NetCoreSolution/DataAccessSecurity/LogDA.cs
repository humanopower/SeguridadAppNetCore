using DataAccessContracts;
using EntityLibrary;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;

namespace DataAccessSecurity
{
	public class LogDA : ILogDA
	{
		#region Propiedades

		private readonly IConfiguration _configuration;
		private string StrConexion { get; set; }

		#endregion Propiedades

		#region Constructor

		public LogDA(IConfiguration configuration)
		{
			_configuration = configuration;
			StrConexion = _configuration.GetConnectionString("SecurityConnectionString");
		}

		#endregion Constructor

		/// <summary>
		/// Metodo que agrega un registro de bitacora en el esquema de seguridad.
		/// </summary>
		/// <param name="log">Objeto tipo Log</param>
		public void AddLogEvent(Log log)
		{
			try
			{
				var conn = StrConexion;
				using (var sqlCon = new SqlConnection(conn))
				{
					sqlCon.Open();
					var cmd = new SqlCommand
					{
						Connection = sqlCon,
						CommandType = System.Data.CommandType.StoredProcedure,
						CommandText = "usp_GeneralLogInsert"
					};

					cmd.Parameters.AddWithValue("@EventTypeId", log.EventTypeId);
					cmd.Parameters.AddWithValue("@EventUserId", log.EventUser.UserId);

					string host = Dns.GetHostName();
					IPHostEntry ip = Dns.GetHostEntry(host);
					log.EventIpAddress = ip.AddressList[0].ToString();
					var ip2 = IPAddress.Parse(log.EventIpAddress);
					cmd.Parameters.AddWithValue("@EventIPAddress", ip2.GetAddressBytes());

					cmd.Parameters.AddWithValue("@ApplicationId", log.Application.ApplicationId);
					cmd.Parameters.AddWithValue("@LogDescription", log.LogDescription);
					cmd.ExecuteNonQuery();
				}
			}
			catch (Exception e)
			{
			}
		}

		/// <summary>
		/// MEtodo que obtiene lista de eventos.
		/// </summary>
		/// <returns>Lista tipo Log</returns>
		public List<Log> GetLogList()
		{
			var conn = StrConexion;
			var logList = new List<Log>();
			using (var sqlCon = new SqlConnection(conn))
			{
				sqlCon.Open();
				var cmd = new SqlCommand
				{
					Connection = sqlCon,
					CommandType = System.Data.CommandType.StoredProcedure,
					CommandText = "usp_LogViewSelect"
				};

				SqlDataReader reader = cmd.ExecuteReader();
				while (reader.Read())
				{
					var log = new Log
					{
						EventId = (int)reader["EventId"]
						 ,
						Application = new ApplicationPMX { ApplicationId = (int)reader["ApplicationId"], ApplicationName = (string)reader["ApplicationName"] }
						 ,
						EventTypeId = (LogTypeEnum)reader["EventTypeId"]
						 ,
						EventDatetime = (DateTime)reader["EventDateTime"]
						 ,
						EventIpAddress = (string)reader["StrIpAddress"]
						 ,
						EventUser = new User { UserId = (string)reader["EventUserId"], EmployeeNumber = (string)reader["EmployeeNumber"], EmployeeNames = (string)reader["EmployeeNames"], EmployeeLastName = (string)reader["EmployeeLastName"] },
					};
					logList.Add(log);
				}
			}
			return logList;
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}
	}
}