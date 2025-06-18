using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using System.Data; 
using top.ebiz.service.Models.Traveler_Profile;

namespace top.ebiz.service.Service.Traveler_Profile
{
    public class logService
    {
        // Log model class
        public class logModel
        {
            public string module { get; set; }
            public string tevent { get; set; }
            public string data_log { get; set; }
            public int ref_id { get; set; }
            public string ref_code { get; set; }
            public string user_log { get; set; }
            public string user_token { get; set; }
        }

        // Method to insert a log entry using stored procedure
        public static int insertLog(logModel value)
        {
            int iResult = -1;

            // Using EF Core DbContext
            using (var context = new TOPEBizEntities())
            {
                // Open the connection to the database
                using (var connection = context.Database.GetDbConnection())
                {
                    connection.Open();

                    // Create a DbCommand to call the stored procedure
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "bz_sp_insert_log";  // Stored procedure name
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Add parameters required by the stored procedure
                        cmd.Parameters.Add(new OracleParameter("p_module", OracleDbType.Varchar2) { Value = value.module });
                        cmd.Parameters.Add(new OracleParameter("p_tevent", OracleDbType.Varchar2) { Value = value.tevent });

                        // Add CLOB parameter for large JSON data
                        cmd.Parameters.Add(new OracleParameter("p_data_log", OracleDbType.Clob) { Value = value.data_log });

                        cmd.Parameters.Add(new OracleParameter("p_ref_id", OracleDbType.Varchar2) { Value = value.ref_id });
                        cmd.Parameters.Add(new OracleParameter("p_ref_code", OracleDbType.Varchar2) { Value = value.ref_code });
                        cmd.Parameters.Add(new OracleParameter("p_user_log", OracleDbType.Varchar2) { Value = value.user_log });
                        cmd.Parameters.Add(new OracleParameter("p_user_token", OracleDbType.Varchar2) { Value = value.user_token });

                        // Execute the stored procedure
                        iResult = cmd.ExecuteNonQuery();
                    }
                }
            }

            return iResult;
        }


        // Method to log out using stored procedure
        public static int insertLogOut(logModel value)
        {
            int iResult = -1;

            // Using EF Core DbContext
            using (var context = new TOPEBizEntities())
            {
                // Open the connection to the database
                using (var connection = context.Database.GetDbConnection())
                {
                    connection.Open();

                    // Create a DbCommand to call the stored procedure
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "bz_sp_logout";  // Stored procedure name
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Add parameter required by the stored procedure
                        cmd.Parameters.Add(new OracleParameter("p_user_token", value.user_token));

                        // Execute the stored procedure
                        iResult = cmd.ExecuteNonQuery();
                    }
                }
            }

            return iResult;
        }

        // Method to insert login token using stored procedure
        public static int insertLogin(loginProfileTravelModel value)
        {
            int iResult = -1;

            // Using EF Core DbContext
            using (var context = new TOPEBizEntities())
            {
                // Open the connection to the database
                using (var connection = context.Database.GetDbConnection())
                {
                    connection.Open();

                    // Create a DbCommand to call the stored procedure
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "BZ_SP_LOGIN_TOKEN_P3";  // Stored procedure name
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Add parameters required by the stored procedure
                        cmd.Parameters.Add(new OracleParameter("p_token_login", value.token_login));
                        cmd.Parameters.Add(new OracleParameter("p_user_id", value.user_id));
                        cmd.Parameters.Add(new OracleParameter("p_user_name", value.user_name));

                        // Execute the stored procedure
                        iResult = cmd.ExecuteNonQuery();
                    }
                }
            }

            return iResult;
        }
    }
}
