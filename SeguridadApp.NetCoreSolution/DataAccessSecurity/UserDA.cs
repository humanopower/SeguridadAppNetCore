using System;
using System.Data;
using System.Linq;
using System.Text;
using DataAccessContracts;
using EntityLibrary;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace DataAccessSecurity
{
    public class UserDA : IUserDA
    { 
        #region Atributos
        private SqlConnection _cnn = null;
        private SqlCommand _cmd = null;
        private SqlDataAdapter _da = null;
        private DataSet _ds = null;
        #endregion

        #region Propiedades
        private string StrConexion { get; set; }
        #endregion

        #region Constructor
        public UserDA()
        {
            StrConexion = StrConexion;
        }
        #endregion

        #region Metodos Publicos
        public  void AddUser(User user, User registerUser)
        {
            var conn = StrConexion;
            using (var sqlCon = new SqlConnection(conn))
            {
                try
                {

                
                sqlCon.Open();
                var cmd = new SqlCommand
                {
                    Connection = sqlCon,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "usp_UsersInsert"
                };
                cmd.Parameters.AddWithValue("@UserId", user.UserId);
                cmd.Parameters.AddWithValue("@EmployeeNumber", user.EmployeeNumber);
                cmd.Parameters.AddWithValue("@EmployeeNames",  user.EmployeeNames.ToUpper());
                cmd.Parameters.AddWithValue("@EmployeeLastName", user.EmployeeLastName.ToUpper());
                cmd.Parameters.AddWithValue("@EmployeeEmail", user.EmployeeEmail);
                cmd.Parameters.AddWithValue("@Telephone",user.Telephone);
                
                var mobile = string.IsNullOrEmpty(user.MobileTelephone) ? " " : user.MobileTelephone;
                cmd.Parameters.AddWithValue("@MobileTelephone", mobile);
                var organismCode = string.IsNullOrEmpty(user.OrganismCode) ? " " : user.OrganismCode;
                    cmd.Parameters.AddWithValue("@OrganismCode", organismCode);
                cmd.Parameters.AddWithValue("@ValidityStartDate", Convert.ToDateTime(string.Format("{0:yyyy/MM/dd}",Convert.ToDateTime(user.ValidityStartDate))));
                cmd.Parameters.AddWithValue("@DeclineDate", Convert.ToDateTime(string.Format("{0:yyyy/MM/dd}",Convert.ToDateTime(user.DeclineDate))));
                cmd.Parameters.AddWithValue("@DeclineDateSIO", Convert.ToDateTime(string.Format("{0:yyyy/MM/dd}",Convert.ToDateTime(user.DeclineDateSIO))));
                //cmd.Parameters.Add("@ValidityStartDate", SqlDbType.Date).Value = Convert.ToDateTime(user.ValidityStartDate);
                //cmd.Parameters.Add("@DeclineDate", SqlDbType.Date).Value = Convert.ToDateTime(user.DeclineDate);
                //cmd.Parameters.Add("@DeclineDateSIO", SqlDbType.Date).Value = Convert.ToDateTime(user.DeclineDateSIO);
                var observations = string.IsNullOrEmpty(user.Observations) ? " " : user.Observations;
                cmd.Parameters.AddWithValue("@Observations", observations);
                cmd.Parameters.AddWithValue("@RegisterUserId", registerUser.UserId);

               
                cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {

                   throw new Exception (ex.Message);
                }
            }
        }

        public  void DelUser(User user, User registerUser)
        {
            var conn = StrConexion;
            using (var sqlCon = new SqlConnection(conn))
            {
                sqlCon.Open();
                var cmd = new SqlCommand
                {
                    Connection = sqlCon,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "usp_UsersDelete"
                };
                cmd.Parameters.Add("@UserId", SqlDbType.NVarChar).Value = user.UserId;
                cmd.Parameters.Add("@EmployeeNumber", SqlDbType.NVarChar).Value = user.EmployeeNumber;
                cmd.Parameters.Add("@DeclineDate", SqlDbType.Date).Value = Convert.ToDateTime(string.Format("{0:yyyy/MM/dd}", Convert.ToDateTime(user.DeclineDate)));
                cmd.Parameters.Add("@RegisterUser", SqlDbType.NVarChar).Value = registerUser.UserId;

                cmd.ExecuteNonQuery();
            }
        }

        public  void UpdUser(User user, User registerUser)
        {
            var conn = StrConexion;
            using (var sqlCon = new SqlConnection(conn))
            {
                sqlCon.Open();
                var cmd = new SqlCommand
                {
                    Connection = sqlCon,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "usp_UsersUpdate"
                };
                cmd.Parameters.Add("@UserId", SqlDbType.NVarChar).Value = user.UserId;
                cmd.Parameters.Add("@EmployeeNumber", SqlDbType.NVarChar).Value = user.EmployeeNumber;
                cmd.Parameters.Add("@EmployeeNames", SqlDbType.NVarChar).Value = user.EmployeeNames.ToUpper();
                cmd.Parameters.Add("@EmployeeLastName", SqlDbType.NVarChar).Value = user.EmployeeLastName.ToUpper();

                cmd.Parameters.Add("@EmployeeEmail", SqlDbType.NVarChar).Value = user.EmployeeEmail;
                cmd.Parameters.Add("@Telephone", SqlDbType.NVarChar).Value = user.Telephone;

                var mobile = string.IsNullOrEmpty(user.MobileTelephone) ? " " : user.MobileTelephone;
                cmd.Parameters.Add("@MobileTelephone", SqlDbType.NVarChar).Value = mobile;
                var organismCode = string.IsNullOrEmpty(user.OrganismCode) ? " " : user.OrganismCode;
                cmd.Parameters.AddWithValue("@OrganismCode", organismCode);
                cmd.Parameters.Add("@ValidityStartDate", SqlDbType.Date).Value = Convert.ToDateTime(string.Format("{0:yyyy/MM/dd}",Convert.ToDateTime(user.ValidityStartDate)));
                cmd.Parameters.Add("@DeclineDate", SqlDbType.Date).Value = Convert.ToDateTime(string.Format("{0:yyyy/MM/dd}", Convert.ToDateTime(user.DeclineDate)));
                cmd.Parameters.Add("@DeclineDateSIO", SqlDbType.Date).Value = Convert.ToDateTime(string.Format("{0:yyyy/MM/dd}", Convert.ToDateTime(user.DeclineDateSIO)));
                var observations = string.IsNullOrEmpty(user.Observations) ? " " : user.Observations;
                cmd.Parameters.Add("@Observations", SqlDbType.NVarChar).Value = observations;
                cmd.Parameters.Add("@RegisterUserId", SqlDbType.NVarChar).Value = registerUser.UserId;
                 

                cmd.ExecuteNonQuery();
            }
        }

        public  User FindUser(string userId)
        {
            var conn = StrConexion;
            using (var sqlCon = new SqlConnection(conn))
            {
                sqlCon.Open();
                var cmd = new SqlCommand
                {
                    Connection = sqlCon,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "usp_UsersSelect"
                };

                cmd.Parameters.Add("@UserId", SqlDbType.NVarChar).Value = userId;
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    var user = new User
                                   {
                                       UserId = (string)reader["UserId"],
                                       EmployeeNumber = (string)reader["EmployeeNumber"]
                                       ,EmployeeNames = (string)reader["EmployeeNames"]
                                       ,EmployeeLastName=(string)reader ["EmployeeLastName"]
                                       ,EmployeeEmail = (string)reader["EmployeeEmail"]
                                       ,Telephone = (string)reader["Telephone"]
                                       //,MobileTelephone = (string)reader["MobileTelephone"]
                                       ,MobileTelephone = string.IsNullOrEmpty(reader["MobileTelephone"].ToString()) ? string.Empty : (string)reader["MobileTelephone"]
                                       ,ValidityStartDate = string.IsNullOrEmpty(reader["ValidityStartDate"].ToString()) ? "01/01/2000" : string.Format("{0:dd/MM/yyyy}", reader["ValidityStartDate"])
                                       ,DeclineDate = string.IsNullOrEmpty(reader["DeclineDate"].ToString()) ? "01/01/2000" : string.Format("{0:dd/MM/yyyy}",reader["DeclineDate"])
                                       ,DeclineDateSIO = string.IsNullOrEmpty(reader["DeclineDateSIO"].ToString()) ? "01/01/2000" : string.Format("{0:dd/MM/yyyy}", reader["DeclineDateSIO"])
                                       ,Observations = (string)reader["Observations"]
                                       ,
                                       RegisterDate = string.IsNullOrEmpty(reader["RegisterDate"].ToString()) ? string.Empty : string.Format("{0:dd/MM/yyyy}", reader["RegisterDate"])
                                       ,LastUpdate = (DateTime)reader["LastUpdate"]
                                       ,
                                       OrganismCode = string.IsNullOrEmpty(reader["OrganismCode"].ToString()) ? string.Empty : (string)reader["OrganismCode"]
                                   };
                    return user;
                }
                return null;
            }
        }

        /// <summary>
        /// Metodo que busca usuarios por distintos campos
        /// </summary>
        /// <param name="strValue">Valor a buscar</param>
        /// <returns>Regresa objeto de tipo DataSet</returns> 
        public DataSet FindUsers(string strValue,User registerUser)
        {
            ConnectDb();

            _cmd = new SqlCommand();
            _da = new SqlDataAdapter();
            _ds = new DataSet();

            _cmd.Connection = _cnn;
            _cmd.CommandType = CommandType.StoredProcedure;
            // Roberto Santos
            //_cmd.CommandText = "usp_BuscaUsuarios";
            _cmd.CommandText = "usp_BuscaUsuariosGral";
            _cmd.Parameters.AddWithValue("@strCampo", strValue);
            _cmd.Parameters.AddWithValue("@UserId", registerUser.UserId);
            _cmd.CommandTimeout = 0;

            _da.SelectCommand = _cmd;
            _da.Fill(_ds);

            DisconnectDb();

            return _ds;
        }

        public  List<User> GetUser(string strValue)
        {
            var conn = StrConexion;
            var lstUserId = new List<User>();
            using (var sqlCon = new SqlConnection(conn))
            {
                sqlCon.Open();
                var cmd = new SqlCommand
                {
                    Connection = sqlCon,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "usp_GetUsers"
                };

                cmd.Parameters.Add("@SearchText", SqlDbType.NVarChar).Value = strValue;
                var da = new SqlDataAdapter();
                var ds = new DataSet();

                da.SelectCommand = cmd;
                da.Fill(ds);

                if (ds.Tables[0].Rows.Count>0)
                {
                    lstUserId.AddRange(from DataRow dr in ds.Tables[0].Rows
                                       select new User
                                                  {
                                                      UserId = dr["EmployeeNames"].ToString()
                                                  });
                }
            }

            return lstUserId;
        }
        public DataSet FindUsersScope(string strValue, User registerUser)
        {
            ConnectDb();

            _cmd = new SqlCommand();
            _da = new SqlDataAdapter();
            _ds = new DataSet();

            _cmd.Connection = _cnn;
            _cmd.CommandType = CommandType.StoredProcedure;
            // Roberto Santos 
            //_cmd.CommandText = "usp_BuscaUsuarios";
            _cmd.CommandText = "usp_BuscaUsuariosConAmbito";
            _cmd.Parameters.AddWithValue("@strCampo", strValue);
            _cmd.Parameters.AddWithValue("@UserId", registerUser.UserId);
            _cmd.CommandTimeout = 0;

            _da.SelectCommand = _cmd;
            _da.Fill(_ds);

            DisconnectDb();

            return _ds;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public string GetNameUserAccount(string numberEmployeeNumber)
        {
            String conn = StrConexion;
            StringBuilder nombreCompleto = new StringBuilder();
            using (var sqlCon = new SqlConnection(conn))
            {
                sqlCon.Open();
                var cmd = new SqlCommand
                {
                    Connection = sqlCon,
                    CommandType = CommandType.StoredProcedure,
                    CommandText = "usp_GetNamesUser"
                };
                cmd.Parameters.AddWithValue("@numeroEmpleado", numberEmployeeNumber);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    nombreCompleto.Append((string)reader["EmployeeNames"]);
                    nombreCompleto.Append(" ");
                    nombreCompleto.Append((string)reader["EmployeeLastName"]);
                }
            }
            return nombreCompleto.ToString();
        }
        #endregion

        #region Metodos Privados
        /// <summary>
        /// Metodo que crea una instancia para enlace con la base de datos
        /// </summary>
        private void ConnectDb()
        {
            _cnn = new SqlConnection(StrConexion);
            _cnn.Open();
        }

        /// <summary>
        /// Metodo que cierra el objeto conexion
        /// </summary>
        private void DisconnectDb()
        {
            if (_cnn.State == ConnectionState.Open)
            {   _cnn.Close(); }
        }

        #endregion

    
    }
}
