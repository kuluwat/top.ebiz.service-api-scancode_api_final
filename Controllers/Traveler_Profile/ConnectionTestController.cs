using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using top.ebiz.service.Service.Traveler_Profile;
using System.Text.Json;
using top.ebiz.service.Service;

namespace top.ebiz.service.Controllers.Traveler_Profile
{
    // [ApiController]
    //[Route("api/[controller]")]
    public class ConnectionTestController : ControllerBase
    {
       

        // POST: api/ConnectionTest
        // [ValidateAntiForgeryToken]
        // [HttpPost("ConnectionTest", Name = "ConnectionTest")]
        // public IActionResult Post([FromBody] string value)
        // {
        //     if (string.IsNullOrEmpty(value))
        //         return BadRequest("Input value cannot be null or empty.");

        //     string ret = string.Empty;
        //     string ConnStrOleDb = value.ToString();

        //     try
        //     {
        //         // Assuming cls_connection is already set up to handle DB operations
        //         ClassConnectionDb conn = new ClassConnectionDb();
        //         conn.OpenConnection();
        //         ret += "*********Ok cls_connection.";

        //         DataTable dt = new DataTable();
        //         string sqlstr = value;

        //         var command = conn.conn.CreateCommand();
        //         command.CommandType = CommandType.StoredProcedure;
        //         command.CommandText = sqlstr;

        //         //if (conn.ExecuteNonQuerySQL(ref dt, sqlstr) == "")
        //         if (conn.ExecuteNonQuerySQL(command) == "")
        //         {
        //             // Perform necessary logic if query executes successfully
        //         }

        //         conn.CloseConnection();

        //         if (dt.Rows.Count > 0)
        //         {
        //             ret += "*********Ok data." + value;
        //             ret += "*********rows data." + dt.Rows[0][0].ToString();
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         ret += "*********error cls_connection: " + ex.Message;
        //     }

        //     // สร้าง IConfiguration เพื่ออ่านไฟล์ appsettings.json
        //     // สร้าง ConfigurationBuilder
        //     var config = new ConfigurationBuilder()
        //         .SetBasePath(Directory.GetCurrentDirectory()) // ระบุเส้นทางปัจจุบันของโปรแกรม
        //         .AddJsonFile("appsettings.json") // เพิ่มไฟล์ JSON ที่ต้องการ
        //         .Build();

        //     // อ่านค่า eBizTravelerProfileConnection
        //     string connectionString = config.GetSection("ConnectionStrings:eBizConnection").Value;
             
        //     ret += "*********connection string : " + connectionString ?? "";

        //     // Log the results using searchDocTravelerProfileServices's logModel
        //     searchDocTravelerProfileServices.logModel log = new searchDocTravelerProfileServices.logModel
        //     {
        //         module = ret
        //     };

        //     // Serialize the log object to JSON
        //     string json = JsonSerializer.Serialize(log);

        //     return Ok(json);
        // }

      

    }
}
