using Microsoft.AspNetCore.Mvc; 

using System.Text.Json;
using top.ebiz.service.Models.Create_Trip;
using top.ebiz.service.Service.Create_Trip;
using top.ebiz.service.Service.Traveler_Profile;

namespace top.ebiz.service.Controllers.ViewLog
{
    // [ApiController]
    //[Route("api/[controller]")]
    public class EmailDetailsController : ControllerBase
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
        [HttpPost("LoadEmailDetails", Name = "LoadEmailDetails")]
        public IActionResult LoadEmailDetails([FromBody] DocDetailSearchModel value)
        { 
            if (value == null) return null;

            // Use System.Text.Json to serialize the object
            var mLog = new logCreateModel { module = "EMAIL", tevent = "SEARCH EMAIL RESEND", ref_id = 0, data_log = JsonSerializer.Serialize(value) };

            // Insert log
           // Service.Create_Trip.logService.insertLog(mLog);

            // Call service method  
            searchDocCreateServices service = new searchDocCreateServices();
            object result = service.SearchEmailDetail(value);

            // Serialize the result to JSON
            var json = JsonSerializer.Serialize(result);

            return Ok(json);
        }


        // POST: api/Controller name  
        [ValidateAntiForgeryToken]
        [HttpPost("SetEmailDetails", Name = "SetEmailDetails")]
        public IActionResult SetEmailDetails([FromBody] DocEmailDetailsSearchModel value)
        {
            if (value == null) return null;

            // Use System.Text.Json to serialize the object
            var mLog = new logCreateModel { module = "EMAIL", tevent = "SEARCH EMAIL RESEND", ref_id = 0, data_log = JsonSerializer.Serialize(value) };

            // Insert log
          //  Service.Create_Trip.logService.insertLog(mLog);

            // Call service method  
            SendEmailServiceTravelerProfile service = new SendEmailServiceTravelerProfile();
            object result = service.updateEmailDetail(value);

            // Serialize the result to JSON
            var json = JsonSerializer.Serialize(result);

            return Ok(json);
        }


    }
}
