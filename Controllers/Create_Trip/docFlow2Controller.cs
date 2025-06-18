
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;  // For JSON Serialization
using top.ebiz.service.Constants;
using top.ebiz.service.Models;
using top.ebiz.service.Models.Create_Trip;
using top.ebiz.service.Service.Create_Trip;

namespace top.ebiz.service.Controllers.Create_Trip
{
    public class DocFlow2Controller : ControllerBase
    {
        // GET: api/docFlow2
        public IEnumerable<string> Get()
        {
            string value = "2019-10-03";
            DateTime? date = DateTime.ParseExact(value, "yyyy-M-d", System.Globalization.CultureInfo.InvariantCulture);
            return new string[] { "value1", "value2" };
        }

        // GET: api/docFlow2/5
        public string Get(int id)
        {
            return "value";
        }
        // [Authorize(AuthenticationSchemes=nameof(Scope.TOP_EBIZ),Roles=nameof(Role.SUPER_ADMIN))]
        [ValidateAntiForgeryToken]
        [HttpPost("docFlow2", Name = "docFlow2")]
        public IActionResult Post([FromBody] DocFlow2Model value)
        {
            if (value == null)
            {
                return BadRequest("Invalid input");
            }

            // Use System.Text.Json to serialize the object
            var mLog = new logCreateModel { module = "DOCUMENT", tevent = "FLOW2", ref_id = 0, data_log = JsonSerializer.Serialize(value) };

            // Insert log
            //logService.insertLog(mLog);

            // Call service method
            var service = new documentService();
            var result = service.submitFlow2(value);

            // Serialize the result to JSON
            var json = JsonSerializer.Serialize(result);
            return Ok(result);
        }


        // PUT: api/docFlow2/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/docFlow2/5
        public void Delete(int id)
        {
        }
    }
}
