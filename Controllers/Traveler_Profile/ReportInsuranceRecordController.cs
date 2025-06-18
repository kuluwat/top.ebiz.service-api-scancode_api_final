using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
//using System.Web.Http;
//using System.Web.Script.Serialization;
using System.Text.Json;
using top.ebiz.service.Models.Traveler_Profile;
using top.ebiz.service.Service.Traveler_Profile;
using static top.ebiz.service.Service.Report.ClassReportModel;

namespace top.ebiz.service.Controllers.Traveler_Profile
{
    // [ApiController]
    //[Route("api/[controller]")]
    public class ReportInsuranceRecordController : ControllerBase
    {
        // GET: api/Controller name

        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Controller name/5

        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Controller name
        [ValidateAntiForgeryToken]
        [HttpPost("ReportInsuranceRecord", Name = "ReportInsuranceRecord")]
        public async Task<IActionResult> ReportInsuranceRecord([FromBody] InsuranceRecordRequest request)
        {
            try
            {
                // Validate input
                if (request == null)
                {
                    return BadRequest(new { error = "Request body cannot be empty" });
                }

                if (string.IsNullOrWhiteSpace(request.token_login))
                {
                    return BadRequest(new { error = "token_login is required" });
                }

                if (string.IsNullOrWhiteSpace(request.year))
                {
                    return BadRequest(new { error = "year is required" });
                }

                // Log the request
                var mLog = new logService.logModel
                {
                    module = "Report",
                    tevent = "report insurance list record",
                    ref_id = 0,
                    data_log = JsonSerializer.Serialize(request)
                };
                //logService.insertLog(mLog);

                // Process the request
                var service = new ExportReportService();
                var exportModel = new ExportRecordModel
                {
                    token_login = request.token_login,
                    year = request.year
                    // Add other properties if needed
                };

                var result = service.report_insurance_list_record(exportModel);

                // Return the response
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the error
                var errorLog = new logService.logModel
                {
                    module = "Report",
                    tevent = "report insurance list record - error",
                    ref_id = 0,
                    data_log = $"Error: {ex.Message}\nStack Trace: {ex.StackTrace}"
                };
                //logService.insertLog(errorLog);

                return StatusCode(500, new { error = "An error occurred while processing your request" });
            }
        }

        // PUT: api/Controller name/5

        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/Controller name/5

        public void Delete(int id)
        {
        }
    }
}
