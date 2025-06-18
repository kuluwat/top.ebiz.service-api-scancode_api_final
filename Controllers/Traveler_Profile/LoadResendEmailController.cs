using Microsoft.AspNetCore.Mvc; 
using System.Text.Json;
using top.ebiz.service.Models.Traveler_Profile;
using top.ebiz.service.Service.Traveler_Profile;

namespace top.ebiz.service.Controllers.Traveler_Profile
{
    // [ApiController]
    //[Route("api/[controller]")]
    public class LoadResendEmailController : ControllerBase
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
        [HttpPost("LoadResendEmail", Name = "LoadResendEmail")]
        public IActionResult Post([FromBody] ResendEmailModel value)
        {
            //if (value == null) return null;

            //searchDocTravelerProfileServices service = new searchDocTravelerProfileServices();
            //logService.logModel mLog = new logService.logModel();

            //mLog.module = "ResendEmail";
            //mLog.tevent = "SearchResendEmail";
            //mLog.ref_id = 0;
            //mLog.data_log = JsonSerializer.Serialize(value);
            ////logService.insertLog(mLog);

            //HttpResponseMessage response = null;
            //Object result = service.SearchResendEmail(value);
            //string json = JsonSerializer.Serialize(result);


            //return Ok(json);
            if (value == null) return BadRequest("Invalid request.");

            var service = new searchDocTravelerProfileServices();
           var mLog = new logService.logModel
            {
                module = "ResendEmail",
                tevent = "SearchResendEmail",
                ref_id = 0,
                data_log = JsonSerializer.Serialize(value)
            };
            // logService.insertLog(mLog);

            try
            {
                var result = service.SearchResendEmail(value);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
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

        //https://www.taithienbo.com/connect-to-oracle-database-from-net-core-application/
    }
}
