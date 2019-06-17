using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using EntityLibrary;
using Microsoft.Extensions.Configuration;

namespace DataAccessSecurity
{
    public class UsersApplicationsRoleDA : IDisposable
    {
		#region Propiedades
		private readonly IConfiguration _configuration;
		private string StrConexion { get; set; }

		#endregion

		#region Constructor

		public UsersApplicationsRoleDA(IConfiguration configuration)
		{
			_configuration = configuration;
			StrConexion = _configuration.GetConnectionString("SecurityConnectionString");
		}

		#endregion

		public List<UsersApplicationsRoles> GetUsersApplicationsRoleList(User user, User loggedUser)
        {
            var conn = StrConexion;
            var userApplicationroleList = new List<UsersApplicationsRoles>();
            using (var sqlCon = new SqlConnection(conn))
            {
                sqlCon.Open();
                var cmd = new SqlCommand
                {
                    Connection = sqlCon,
                    CommandType = CommandType.StoredProcedure,
                    //Roberto Santos 
                    //CommandText = "usp_GetUserOperationRole"
                    CommandText = "usp_GetUserOperationRoleConAmbito"
                };
                cmd.Parameters.AddWithValue("@UserId", user.UserId);
                cmd.Parameters.AddWithValue("@LoggedUserId", loggedUser.UserId);           
                var da = new SqlDataAdapter();
                var ds = new DataSet();

                da.SelectCommand = cmd;
                da.Fill(ds);

                if (ds.Tables[0].Rows.Count>0)
                {
                    userApplicationroleList.AddRange(from DataRow dr in ds.Tables[0].Rows
                                                     select new UsersApplicationsRoles
                                                                {
                                                                    UserId = (string) dr["UserId"], 
                                                                    EmployeeNames = (string) dr["EmployeeNames"], 
                                                                    ApplicationId = (int) dr["ApplicationId"], 
                                                                    ApplicationName = (string) dr["ApplicationName"], 
                                                                    RoleId = (int) dr["RoleId"], 
                                                                    RoleName = (string) dr["RoleName"], 
                                                                    CreationDateTime = (DateTime) dr["CreationDateTime"], 
                                                                    CreationUserId = string.IsNullOrEmpty(dr["CreationUserId"].ToString()) ? string.Empty : dr["CreationUserId"].ToString(), 
                                                                    ModificationDateTime = string.IsNullOrEmpty(dr["ModificationDateTime"].ToString()) ? string.Empty : string.Format("{0:dd/MM/yyyy}", dr["ModificationDateTime"]), 
                                                                    ModificationUserId = string.IsNullOrEmpty(dr["ModificationUserId"].ToString()) ? string.Empty : dr["ModificationUserId"].ToString(),
                                                                    DeclineDate = string.IsNullOrEmpty(dr["DeclineDate"].ToString()) ? string.Empty : string.Format("{0:dd/MM/yyyy}", dr["DeclineDate"])
                                                                });
                }
            }
            return userApplicationroleList;
        }

        public  void AddNewUsersApplicationsRoles(UsersApplicationsRoles usersApplicationsRoles, User registerUser)
        {
            var conn = StrConexion;
            using (var sqlCon = new SqlConnection(conn))
            {
                sqlCon.Open();
                var cmd = new SqlCommand
                {
                    Connection = sqlCon,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "usp_UsersApplicationsRolesInsert"
                };
                cmd.Parameters.AddWithValue("@UserId", usersApplicationsRoles.UserId);
                cmd.Parameters.AddWithValue("@ApplicationId", usersApplicationsRoles.ApplicationId);
                cmd.Parameters.AddWithValue("@RoleId", usersApplicationsRoles.RoleId);
                cmd.Parameters.AddWithValue("@CreationUserId", registerUser.UserId);
                //cmd.Parameters.AddWithValue("@DeclineDate", Convert.ToDateTime(usersApplicationsRoles.DeclineDate));
                cmd.Parameters.AddWithValue("@DeclineDate", Convert.ToDateTime(string.Format("{0:yyyy/MM/dd}", Convert.ToDateTime(usersApplicationsRoles.DeclineDate))));
                
                cmd.ExecuteNonQuery();
            }
        }

        public  void DeleteUsersApplicationsRoles(UsersApplicationsRoles usersApplicationsRoles, User registerUser)
        {
            var conn = StrConexion;
            using (var sqlCon = new SqlConnection(conn))
            {
                sqlCon.Open();
                var cmd = new SqlCommand
                {
                    Connection = sqlCon,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "usp_UsersApplicationsRolesDelete"
                };
                cmd.Parameters.AddWithValue("@UserId", usersApplicationsRoles.UserId);
                cmd.Parameters.AddWithValue("@ApplicationName", usersApplicationsRoles.ApplicationName);
                cmd.Parameters.AddWithValue("@RoleName", usersApplicationsRoles.RoleName);
                cmd.Parameters.AddWithValue("@DeclineDate", Convert.ToDateTime(usersApplicationsRoles.DeclineDate));
                cmd.Parameters.AddWithValue("@ModificationUserId", registerUser.UserId);

                cmd.ExecuteNonQuery();
            }
        }

        public  List<UsersApplicationsRoles> GetApplicationRoleList(User user,User loggedUser)
        {
            var conn = StrConexion;
            var lstAppUserRol = new List<UsersApplicationsRoles>();

            using (var sqlCon = new SqlConnection(conn))
            {
                sqlCon.Open();
                var cmd = new SqlCommand
                {
                    Connection = sqlCon,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "usp_UsersGetApplicationRolesConAmbito"
                };
                cmd.Parameters.AddWithValue("@UserId", user.UserId);
                cmd.Parameters.AddWithValue("@LoggedUserId", loggedUser.UserId);

                var da = new SqlDataAdapter();
                var ds = new DataSet();

                da.SelectCommand = cmd;
                da.Fill(ds);

                if (ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                       var objAppUserRole = new UsersApplicationsRoles();

                        objAppUserRole.ApplicationId = Convert.ToInt32(dr["ApplicationId"]);
                        objAppUserRole.ApplicationName = dr["ApplicationName"].ToString();
                        objAppUserRole.RoleName = dr["RoleName"].ToString();
                        objAppUserRole.ApplicationDescription = dr["ApplicationDescription"].ToString();
                        if (!dr["ValidityStartDate"].ToString().Equals(string.Empty))
                        { objAppUserRole.ValidityStartDate = string.Format("{0:dd/MM/yyyy}", Convert.ToDateTime(dr["ValidityStartDate"])); }
                        if (!dr["DeclineDate"].ToString().Equals(string.Empty))
                        { objAppUserRole.DeclineDate = string.Format("{0:dd/MM/yyyy}", Convert.ToDateTime(dr["DeclineDate"])); }
                  
                        lstAppUserRol.Add(objAppUserRole);
                    }
                }
            }
            return lstAppUserRol;
        }

        public  List<User> FindRoleUsers(Role role, ApplicationPMX application)
        {
            var conn = StrConexion;
            using (var sqlCon = new SqlConnection(conn))
            {
                sqlCon.Open();
                var cmd = new SqlCommand
                {
                    Connection = sqlCon,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "usp_UsersSelectByApplicationIdAndRoleId"
                };

                cmd.Parameters.Add("@ApplicationId", SqlDbType.NVarChar).Value = application.ApplicationId;
                cmd.Parameters.Add("@RoleId", SqlDbType.Int).Value = role.RoleId;
                SqlDataReader reader = cmd.ExecuteReader();
                var userList = new List<User>();
                while (reader.Read())
                {
                    var user = new User
                                   {
                                       UserId = (string)reader["UserId"],
                                       EmployeeNumber = (string)reader["EmployeeNumber"]
                                       ,EmployeeNames = (string)reader["EmployeeNames"]
                                       ,EmployeeLastName=(string)reader ["EmployeeLastName"]
                                       ,EmployeeEmail = (string)reader["EmployeeEmail"]
                                       ,Telephone = (string)reader["Telephone"]
                                       ,MobileTelephone = (string)reader["MobileTelephone"]
                                      ,DeclineDate = reader["DeclineDate"] == System.DBNull.Value ? "01/01/2000" : reader["DeclineDate"].ToString()
                                       ,DeclineDateSIO = reader["DeclineDateSIO"] == System.DBNull.Value ? "01/01/2000" : reader["DeclineDateSIO"].ToString()
                                   };
                   
                    
                    userList.Add(user);
                    
                }
                return userList;
            }
        }

        public  List<UsersApplicationsRoles> GetApplicationUsersList(ApplicationPMX applicationPMX)
        {
            var conn = StrConexion;
            var lstAppUserRol = new List<UsersApplicationsRoles>();

            using (var sqlCon = new SqlConnection(conn))
            {
                sqlCon.Open();
                var cmd = new SqlCommand
                {
                    Connection = sqlCon,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "usp_ApplicationUsersByApplicationId"
                };
                cmd.Parameters.AddWithValue("@ApplicationId", applicationPMX.ApplicationId);

                var da = new SqlDataAdapter();
                var ds = new DataSet();

                da.SelectCommand = cmd;
                da.Fill(ds);

                if (ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        var objAppUserRole = new UsersApplicationsRoles();
                        objAppUserRole.UserId = dr["UserId"].ToString();
                        objAppUserRole.ApplicationId = Convert.ToInt32(dr["ApplicationId"]);
                        objAppUserRole.ApplicationName = dr["ApplicationName"].ToString();
                        lstAppUserRol.Add(objAppUserRole);
                    }
                }
            }
            return lstAppUserRol;
        }





        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
