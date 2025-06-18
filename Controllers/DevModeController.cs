
using Microsoft.AspNetCore.Mvc;
using System.Data;
using top.ebiz.service.Service.Traveler_Profile;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using top.ebiz.service.Constants;

namespace ebiz.webservice.service.Controllers
{
    [Authorize(Policy = nameof(PolicyEbiz.READ_WRITE_ALL))]
    public class DevModeController : ControllerBase
    {
        public DevModeController()
        {
        }

        [IgnoreAntiforgeryToken]
        [HttpPost("ExecuteDataMode", Name = "ExecuteDataMode")]
        public IActionResult ExecuteDataMode(string sqlstr)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sqlstr))
                {
                    return BadRequest("SQL query is required.");
                }

                DataTable dt = new DataTable();
                var ret = SetDocService.conn_ExecuteData(ref dt, sqlstr);

                if (!string.IsNullOrEmpty(ret))
                {
                    return BadRequest(new { error = ret });
                }

                // ใช้ Newtonsoft.Json เพื่อ Serialize DataTable
                var json = JsonConvert.SerializeObject(dt, Formatting.Indented);
                return Ok(json);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }


    }
}
