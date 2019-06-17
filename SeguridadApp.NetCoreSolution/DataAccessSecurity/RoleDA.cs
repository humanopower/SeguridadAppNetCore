using System;
using System.Data;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using DataAccessContracts;
using EntityLibrary;
using Microsoft.Extensions.Configuration;

namespace DataAccessSecurity
{
    public class RoleDA : IRoleDA
    {
        #region Atributos
        private static DataTable _dtFixed = null;
        private static DataRow _drFixed;
		#endregion
		#region Propiedades
		private readonly IConfiguration _configuration;
		private string StrConexion { get; set; }

		#endregion

		#region Constructor

		public RoleDA(IConfiguration configuration)
		{
			_configuration = configuration;
			StrConexion = _configuration.GetConnectionString("SecurityConnectionString");
		}

		#endregion

		public void AddRole(Role role,User registerUser )
        {
            var conn = StrConexion;
            
            using (var sqlCon = new SqlConnection(conn))
            {
                sqlCon.Open();
                var cmd = new SqlCommand
                {
                    Connection = sqlCon,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "usp_RolesInsert"
                };
                cmd.Parameters.AddWithValue("@RoleName", role.RoleName);
                cmd.Parameters.AddWithValue("@RoleDescription", role.RoleDescription);
                cmd.Parameters.AddWithValue("@ApplicationId", role.ApplicationId);        
                cmd.Parameters.AddWithValue("@CreationUserId", registerUser.UserId);
                cmd.Parameters.AddWithValue("@ModificationUserId", registerUser.UserId);
                cmd.Parameters.AddWithValue("@DeclineDate", Convert.ToDateTime(string.Format("{0:yyyy/MM/dd}", Convert.ToDateTime(role.DeclineDate))));
                cmd.Parameters.AddWithValue("@RoleAuthorizationUserId", role.RoleAuthorizationUserId);
                cmd.Parameters.AddWithValue("@RoleAuthorizationOwner", role.RoleAuthorizationOwner);

                cmd.Parameters.Add("@RoleId", SqlDbType.Int).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                role.RoleId = (int)cmd.Parameters["@RoleId"].Value;
            }
        }

        public  void UpdRole(Role role, User registerUser)
        {
            var conn = StrConexion;

            using (var sqlCon = new SqlConnection(conn))
            {
                sqlCon.Open();
                var cmd = new SqlCommand
                {
                    Connection = sqlCon,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "usp_RolesUpdate"
                };

                cmd.Parameters.AddWithValue("@RoleId", role.RoleId);
                cmd.Parameters.AddWithValue("@RoleName", role.RoleName);
                cmd.Parameters.AddWithValue("@RoleDescription", role.RoleDescription);
                cmd.Parameters.AddWithValue("@ApplicationId", role.ApplicationId);
                cmd.Parameters.AddWithValue("@ModificationUserId", registerUser.UserId);
                cmd.Parameters.AddWithValue("@DeclineDate", Convert.ToDateTime(string.Format("{0:yyyy/MM/dd}", Convert.ToDateTime(role.DeclineDate))));
                cmd.Parameters.AddWithValue("@RoleAuthorizationUserId", role.RoleAuthorizationUserId);
                cmd.Parameters.AddWithValue("@RoleAuthorizationOwner", role.RoleAuthorizationOwner);
                
                cmd.ExecuteNonQuery();
            }
        }

        public  void DelRole(Role role, User registerUser)
        {
            var conn = StrConexion;

            using (var sqlCon = new SqlConnection(conn))
            {
                sqlCon.Open();
                var cmd = new SqlCommand
                {
                    Connection = sqlCon,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "usp_RolesDelete"
                };

                cmd.Parameters.AddWithValue("@RoleId", role.RoleId);
                cmd.Parameters.AddWithValue("@ApplicationId", role.ApplicationId);
                cmd.Parameters.AddWithValue("@DeclineDate", Convert.ToDateTime(string.Format("{0:yyyy/MM/dd}",  Convert.ToDateTime(role.DeclineDate))));
                cmd.Parameters.AddWithValue("@ModificationUserId", registerUser.UserId);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Metodo que obtiene la lista de roles asignados a un usuario previamente registrado.
        /// </summary>
        /// <param name="application">Objeto Application</param>
        /// <param name="user"> Objeto User</param>
        /// <returns>Lista de roles</returns>
        public  List<Role> GetRoleList(ApplicationPMX application, User user)
        {
            string conn = StrConexion;
            var roleList = new List<Role>();
            using (var sqlCon = new SqlConnection(conn))
            {
                sqlCon.Open();
                var cmd = new SqlCommand
                {
                    Connection = sqlCon,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "usp_ApplicationRolesSelectByUserIdAndApplicationId"
                };
                cmd.Parameters.AddWithValue("@UserId", user.UserId);
                cmd.Parameters.AddWithValue("@ApplicationId", application.ApplicationId);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var role = new Role
                     {
                        RoleId = (int) reader["RoleId"]
                        ,
                        RoleName = (string)reader["RoleName"]
                        ,
                        RoleDescription = (string)reader["RoleDescription"]
                        ,
                        ApplicationId = (int)reader["ApplicationId"]
                    };
                    roleList.Add(role);
                }
            }
            return roleList;
        }
        
        public  Role GetRole(int roleId)
        {
            var conn = StrConexion;
            var objRole = new Role();
            using (var sqlCon = new SqlConnection(conn))
            {
                sqlCon.Open();
                var cmd = new SqlCommand
                {
                    Connection = sqlCon,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "usp_FindRole"
                };
                cmd.Parameters.AddWithValue("@RoleId", roleId);

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var role = new Role
                    {
                        RoleId = (int)reader["RoleId"],
                        RoleName = (string)reader["RoleName"],
                        RoleDescription = (string)reader["RoleDescription"],
                        ApplicationId = (int)reader["ApplicationId"],
                        DeclineDate = string.IsNullOrEmpty(reader["DeclineDate"].ToString()) ? string.Empty : string.Format("{0:dd/MM/yyyy}",reader["DeclineDate"]),
                        RoleAuthorizationUserId = reader["RoleAuthorizationUserId"].ToString(),
                        RoleAuthorizationOwner = reader["RoleAuthorizationOwner"].ToString()
                    };
                    objRole = role;
                }
            }

            return objRole;
        }

        public  List<Role> GetRoles(string strValue)
        {
            string conn = StrConexion;
            var roleList = new List<Role>();
            using (var sqlCon = new SqlConnection(conn))
            {
                sqlCon.Open();
                var cmd = new SqlCommand
                {
                    Connection = sqlCon,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "usp_FindRoles"
                };
                cmd.Parameters.AddWithValue("@strCampo", strValue);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var role = new Role
                    {
                        RoleId = (int)reader["RoleId"],
                        RoleName = (string)reader["RoleName"],
                        RoleDescription = (string)reader["RoleDescription"],
                        ApplicationId = (int)reader["ApplicationId"]
                    };
                    roleList.Add(role);
                }
            }

            return roleList;
        }

        public  DataTable GetRolesApplications(string strValue)
        {
            CreateColumns();

            var conn = StrConexion;
            var roleList = new List<Role>();
           
            using (var sqlCon = new SqlConnection(conn))
            {
                sqlCon.Open();
                var cmd = new SqlCommand
                {
                    Connection = sqlCon,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "usp_FindRoles"
                };
                cmd.Parameters.AddWithValue("@strCampo", strValue);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var role = new Role
                    {
                        RoleId = (int)reader["RoleId"],
                        RoleName = (string)reader["RoleName"],
                        RoleDescription = (string)reader["RoleDescription"],
                        ApplicationId = (int)reader["ApplicationId"],
                        DeclineDate = string.IsNullOrEmpty(reader["DeclineDate"].ToString()) ? string.Empty : string.Format("{0:dd/MM/yyyy}", reader["DeclineDate"])
                    };
                    roleList.Add(role);
                }

                ApplicationDA applicationDa = new ApplicationDA(_configuration);
                //llenamos Tabla
                if (roleList.Count > 0)
                {
                    foreach (var role in roleList)
                    {
                        _drFixed = _dtFixed.NewRow();

                        _drFixed["RoleId"] = role.RoleId;
                        _drFixed["RoleName"] = role.RoleName;
                        _drFixed["RoleDescription"] = role.RoleDescription;
                        _drFixed["ApplicationId"] = role.ApplicationId;

                        _drFixed["ApplicationName"] = applicationDa.FindApplication(role.ApplicationId).ApplicationName;
                        _drFixed["DeclineDate"] = role.DeclineDate;

                        _dtFixed.Rows.Add(_drFixed);
                    }
                }
                applicationDa.Dispose();
            }

            return _dtFixed;
        }

        public  List<Role> GetAllroles()
        {
            var conn = StrConexion;
            var lstRole = new List<Role>();
            using (var sqlCon = new SqlConnection(conn))
            {
                sqlCon.Open();
                var cmd = new SqlCommand
                {
                    Connection = sqlCon,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "usp_GetAllRoles"
                };
                
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var role = new Role
                    {
                        RoleId = (int)reader["RoleId"],
                        RoleName = (string)reader["RoleName"],
                        RoleDescription = (string)reader["RoleDescription"],
                        ApplicationId = (int)reader["ApplicationId"]
                    };
                    lstRole.Add(role);
                }
            }

            return lstRole;
        }

        /// <summary>
        /// Obtiene lista de roles por aplicacion
        /// </summary>
        /// <param name="userApplicationRole">Objeto tipo Aplicacion</param>
        /// /// <param name="iTipo">Tipo de Aplicacion</param>
        /// <returns>Lista de roles</returns>
        public  List<Role> GetRoleforApplication(UsersApplicationsRoles userApplicationRole, int iTipo)
        {

            var conn = StrConexion;
            var rolelst = new List<Role>();
            using (var sqlCon = new SqlConnection(conn))
            {
                sqlCon.Open();
                var cmd = new SqlCommand
                {
                    Connection = sqlCon,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "usp_FindRolesforApplication"
                };
                cmd.Parameters.AddWithValue("@IdApplication", userApplicationRole.ApplicationId);
                cmd.Parameters.AddWithValue("@UserId", userApplicationRole.UserId);
                cmd.Parameters.AddWithValue("@Tipo", iTipo);

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var objRole = new Role
                                      {
                        RoleId = (int)reader["RoleId"],
                        RoleName = (string)reader["RoleName"],
                        RoleDescription = string.IsNullOrEmpty(reader["RoleDescription"].ToString()) ? string.Empty : reader["RoleDescription"].ToString(),
                        ApplicationId = (int)reader["ApplicationId"],
                        CreationDateTime = (DateTime)reader["CreationDateTime"],
                        CreationUserId = string.IsNullOrEmpty(reader["CreationUserId"].ToString()) ? string.Empty : reader["CreationUserId"].ToString(),
                        ModificationDateTime = (DateTime)reader["ModificationDateTime"],
                        ModificationUserId = string.IsNullOrEmpty(reader["ModificationUserId"].ToString()) ? string.Empty : reader["ModificationUserId"].ToString(),
                        DeclineDate = string.IsNullOrEmpty(reader["DeclineDate"].ToString()) ? string.Empty : string.Format("{0:dd/MM/yyy}", reader["DeclineDate"]),
                        RoleAuthorizationUserId = (string)reader["RoleAuthorizationUserId"],
                        RoleAuthorizationOwner = (string)reader["RoleAuthorizationOwner"]
                    };
                    rolelst.Add(objRole);
                }
            }

            return rolelst;
        }

        public  void UpdateRoleOperations(ApplicationPMX application, Role role, List<Operation> operations)
        {
        }

        public  DataTable GetRoleApplications(UsersApplicationsRoles userApplicationRole, int tipo)
        {
            CreateColumns();

            var lstRole = GetRoleforApplication(userApplicationRole, tipo);
            ApplicationDA applicationDa = new ApplicationDA(_configuration);
            if (lstRole.Count>0)
            {
                foreach (var role in lstRole)
                {
                    _drFixed = _dtFixed.NewRow();
                    
                    _drFixed["RoleId"] = role.RoleId;
                    _drFixed["RoleName"] = role.RoleName;
                    _drFixed["RoleDescription"] = role.RoleDescription;
                    _drFixed["ApplicationId"] = role.ApplicationId;
                    _drFixed["ApplicationName"] = applicationDa.FindApplication(role.ApplicationId).ApplicationName; 
                    _drFixed["CreationDateTime"] = role.CreationDateTime;
                    _drFixed["CreationUserId"] = role.CreationUserId;
                    _drFixed["ModificationDateTime"] = role.ModificationDateTime;
                    _drFixed["ModificationUserId"] = role.ModificationUserId;
                    _drFixed["DeclineDate"] = role.DeclineDate;
                    _drFixed["RoleAuthorizationUserId"] = role.RoleAuthorizationUserId;
                    _drFixed["RoleAuthorizationOwner"] = role.RoleAuthorizationOwner;

                    _dtFixed.Rows.Add(_drFixed);
                }
            }
            applicationDa.Dispose();

            return _dtFixed;

        }

        public void CreateColumns()
        {
            _dtFixed = new DataTable();

            try
            {
                var column = new DataColumn
                                 {
                                     DataType = Type.GetType("System.Int32"),
                                     ColumnName = "RoleId",
                                     ReadOnly = false
                                 };
                _dtFixed.Columns.Add(column);

                column = new DataColumn
                             {
                                 DataType = Type.GetType("System.String"),
                                 ColumnName = "RoleName",
                                 ReadOnly = false
                             };
                _dtFixed.Columns.Add(column);

                column = new DataColumn
                             {
                                 DataType = Type.GetType("System.String"),
                                 ColumnName = "RoleDescription",
                                 ReadOnly = false
                             };
                _dtFixed.Columns.Add(column);

                column = new DataColumn
                             {
                                 DataType = Type.GetType("System.Int32"),
                                 ColumnName = "ApplicationId",
                                 ReadOnly = false
                             };
                _dtFixed.Columns.Add(column);

                column = new DataColumn
                             {
                                 DataType = Type.GetType("System.String"),
                                 ColumnName = "ApplicationName",
                                 ReadOnly = false
                             };
                _dtFixed.Columns.Add(column);

                column = new DataColumn
                             {
                                 DataType = Type.GetType("System.DateTime"),
                                 ColumnName = "CreationDateTime",
                                 ReadOnly = false
                             };
                _dtFixed.Columns.Add(column);

                column = new DataColumn
                             {
                                 DataType = Type.GetType("System.String"),
                                 ColumnName = "CreationUserId",
                                 ReadOnly = false
                             };
                _dtFixed.Columns.Add(column);

                column = new DataColumn
                             {
                                 DataType = Type.GetType("System.DateTime"),
                                 ColumnName = "ModificationDateTime",
                                 ReadOnly = false
                             };
                _dtFixed.Columns.Add(column);

                column = new DataColumn
                             {
                                 DataType = Type.GetType("System.String"),
                                 ColumnName = "ModificationUserId",
                                 ReadOnly = false
                             };
                _dtFixed.Columns.Add(column);

                column = new DataColumn
                             {
                                 DataType = Type.GetType("System.String"),
                                 ColumnName = "DeclineDate",
                                 ReadOnly = false
                             };
                _dtFixed.Columns.Add(column);

                column = new DataColumn
                             {
                                 DataType = Type.GetType("System.String"),
                                 ColumnName = "RoleAuthorizationUserId",
                                 ReadOnly = false
                             };
                _dtFixed.Columns.Add(column);

                column = new DataColumn
                             {
                                 DataType = Type.GetType("System.String"),
                                 ColumnName = "RoleAuthorizationOwner",
                                 ReadOnly = false
                             };
                _dtFixed.Columns.Add(column);
            }
            catch (Exception ex)
            { throw new Exception(ex.Message); }
        }
        
        public  List<Role> GetRoleList(ApplicationPMX application)
        {
            var conn = StrConexion;
            var roleList = new List<Role>();
            using (var sqlCon = new SqlConnection(conn))
            {
                sqlCon.Open();
                var cmd = new SqlCommand
                {
                    Connection = sqlCon,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "usp_ApplicationRolesSelectByApplicationId"
                };
               
                cmd.Parameters.AddWithValue("@ApplicationId", application.ApplicationId);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var role = new Role
                    {
                        RoleId = (int)reader["RoleId"]
                       ,
                        RoleName = (string)reader["RoleName"]
                       ,
                        RoleDescription = (string)reader["RoleDescription"]
                       ,
                        ApplicationId = (int)reader["ApplicationId"]
                    };
                    roleList.Add(role);
                }
            }
            return roleList;
        }

        public  void InsertRoleNotAllowedCombination(ApplicationPMX application, Role roleA, Role roleB, User registerUser)
        {
            var conn = StrConexion;
            using (var sqlCon = new SqlConnection(conn))
            {
                sqlCon.Open();
                var cmd = new SqlCommand
                {
                    Connection = sqlCon,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "usp_RolesCombinationNotAllowedInsert"
                };
                cmd.Parameters.AddWithValue("@ApplicationId", application.ApplicationId);
                cmd.Parameters.AddWithValue("@RoleIdA", roleA.RoleId);
                cmd.Parameters.AddWithValue("@RoleIdB", roleB.RoleId);
                cmd.Parameters.AddWithValue("@UserId", registerUser.UserId);
                cmd.ExecuteNonQuery();
            }
        }

        public  void DeleteRoleNotAllowedCombination(ApplicationPMX application, Role roleA, Role roleB)
        {
            var conn = StrConexion;
            using (var sqlCon = new SqlConnection(conn))
            {
                sqlCon.Open();
                var cmd = new SqlCommand
                {
                    Connection = sqlCon,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "usp_RolesCombinationNotAllowedDelete"
                };
                cmd.Parameters.AddWithValue("@ApplicationId", application.ApplicationId);
                cmd.Parameters.AddWithValue("@RoleIdA", roleA.RoleId);
                cmd.Parameters.AddWithValue("@RoleIdB", roleB.RoleId);
                cmd.ExecuteNonQuery();
            }
        }

        public  bool RoleNotAllowedCombinationExist(ApplicationPMX application, Role roleA, Role roleB)
        {
            var conn = StrConexion;
            using (var sqlCon = new SqlConnection(conn))
            {
                sqlCon.Open();
                var cmd = new SqlCommand
                {
                    Connection = sqlCon,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "usp_RolesCombinationNotAllowedSelectByApplicationIdAndRoles"
                };
                cmd.Parameters.AddWithValue("@ApplicationId", application.ApplicationId);
                cmd.Parameters.AddWithValue("@RoleIdA", roleA.RoleId);
                cmd.Parameters.AddWithValue("@RoleIdB", roleB.RoleId);
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return true;
                }
            }
            return false;
        }
        public  bool RoleNotAllowedCombinationExistAndDate(ApplicationPMX application, Role roleA, Role roleB)
        {
            var conn = StrConexion;
            using (var sqlCon = new SqlConnection(conn))
            {
                sqlCon.Open();
                var cmd = new SqlCommand
                {
                    Connection = sqlCon,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "usp_RolesCombinationNotAllowedSelectByApplicationIdAndRolesAndDate"
                };
                cmd.Parameters.AddWithValue("@ApplicationId", application.ApplicationId);
                cmd.Parameters.AddWithValue("@RoleIdA", roleA.RoleId);
                cmd.Parameters.AddWithValue("@RoleIdB", roleB.RoleId);
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return true;
                }
            }
            return false;
        }

        public  DataTable GetRoleCombinationsNotAllowed(ApplicationPMX application)
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
                    CommandText = "usp_RolesCombinationNotAllowedSelectByApplicationId"
                };
                cmd.Parameters.AddWithValue("@ApplicationId", application.ApplicationId);
                SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);

            }
            return dt;
        }


        public void UpdateRoleNotAllowedCombination(ApplicationPMX application, Role roleA, Role roleB, DateTime dtDeclineDate, User registerUser)
        {
            var conn = StrConexion;
            using (var sqlCon = new SqlConnection(conn))
            {
                sqlCon.Open();
                var cmd = new SqlCommand
                {
                    Connection = sqlCon,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "usp_RolesCombinationNotAllowedUpdate"
                };
                cmd.Parameters.AddWithValue("@ApplicationID", application.ApplicationId);
                cmd.Parameters.AddWithValue("@RoleIdA", roleA.RoleId);
                cmd.Parameters.AddWithValue("@RoleIdB", roleB.RoleId);
                cmd.Parameters.AddWithValue("@UserId", registerUser.UserId);
                cmd.Parameters.AddWithValue("@DeclineDate", dtDeclineDate);
                cmd.ExecuteNonQuery();
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
