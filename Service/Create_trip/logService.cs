using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client; 
using System.Data;
using top.ebiz.service.Models.Create_Trip;

namespace top.ebiz.service.Service.Create_Trip
{
    public class logService
    {
        public static int insertLog(logCreateModel value)
        {
            int iResult = -1;
            try
            {
                // ใช้ EF Core DbContext
                using (var context = new TOPEBizCreateTripEntities())
                {
                    // เปิดการเชื่อมต่อกับฐานข้อมูล
                    using (var connection = context.Database.GetDbConnection())
                    {
                        connection.Open();

                        // สร้าง DbCommand เพื่อเรียก stored procedure
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "bz_sp_insert_log";
                            cmd.CommandType = CommandType.StoredProcedure;

                            // เพิ่มพารามิเตอร์ต่าง ๆ ที่ต้องการ
                            cmd.Parameters.Add(new OracleParameter("p_module", value.module));
                            cmd.Parameters.Add(new OracleParameter("p_event", value.tevent));
                            cmd.Parameters.Add(new OracleParameter("p_data_log", value.data_log));
                            cmd.Parameters.Add(new OracleParameter("p_ref_id", value.ref_id));
                            cmd.Parameters.Add(new OracleParameter("p_ref_code", value.ref_code));
                            cmd.Parameters.Add(new OracleParameter("p_user_log", value.user_log));
                            cmd.Parameters.Add(new OracleParameter("p_user_token", value.user_token));

                            // เรียก ExecuteNonQuery เพื่อดำเนินการ stored procedure
                            iResult = cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex) { }
            return iResult;
        }
         
    }
}
