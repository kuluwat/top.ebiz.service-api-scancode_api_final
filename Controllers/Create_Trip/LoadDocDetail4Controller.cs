using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using top.ebiz.service.Service.Create_Trip;
using top.ebiz.service.Models.Create_Trip;

namespace top.ebiz.service.Controllers.Create_Trip
{ 
    public class LoadDocDetail4Controller : ControllerBase
    { 
        // GET: api/LoadDocDetail4
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/LoadDocDetail4/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/LoadDocDetail4
        [ValidateAntiForgeryToken]
        [HttpPost("LoadDocDetail4", Name = "LoadDocDetail4")]
        public IActionResult Post([FromBody] DocDetail3Model value)
        {
            if (value == null) return null;
              
            // Use System.Text.Json to serialize the object
            var mLog = new logCreateModel { module = "DOCUMENT", tevent = "SEARCH DETAIL 4", ref_id = 0, data_log = JsonSerializer.Serialize(value) };

            // Insert log
            //logService.insertLog(mLog);

            // Call service method  
            searchDocCreateServices service = new searchDocCreateServices();
            object result = service.SearchDetail4(value);

            // Serialize the result to JSON
            var json = JsonSerializer.Serialize(result);
             
            return Ok(json);
        }

        // PUT: api/LoadDocDetail4/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/LoadDocDetail4/5
        public void Delete(int id)
        {
        }
    }
}
