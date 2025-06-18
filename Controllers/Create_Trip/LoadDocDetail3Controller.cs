using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using top.ebiz.service.Service.Create_Trip;
using top.ebiz.service.Models.Create_Trip;
namespace top.ebiz.service.Controllers.Create_Trip
{ 
    public class LoadDocDetail3Controller : ControllerBase
    { 
        // GET: api/LoadDocDetail3
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/LoadDocDetail3/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/LoadDocDetail3
        [ValidateAntiForgeryToken]
        [HttpPost("LoadDocDetail3", Name = "LoadDocDetail3")]
        public IActionResult Post([FromBody] DocDetail3Model value)
        {
            if (value == null) return null;
             
            // Use System.Text.Json to serialize the object
            var mLog = new logCreateModel { module = "DOCUMENT", tevent = "SEARCH DETAIL 3", ref_id = 0, data_log = JsonSerializer.Serialize(value) };

            // Insert log
            //logService.insertLog(mLog);

            // Call service method   
            searchDocCreateServices service = new searchDocCreateServices();
            object result = service.SearchDetail3(value);

            // Serialize the result to JSON
            var json = JsonSerializer.Serialize(result);
             

            return Ok(json);

        }

        // PUT: api/LoadDocDetail3/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/LoadDocDetail3/5
        public void Delete(int id)
        {
        }
    }
}
