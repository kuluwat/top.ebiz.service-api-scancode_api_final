using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using top.ebiz.service.Service.Create_Trip;
using top.ebiz.service.Models.Create_Trip;

namespace top.ebiz.service.Controllers.Create_Trip
{ 
    public class LoadDocDetail2Controller : ControllerBase
    {  
        // GET: api/LoadDocDetail2
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/LoadDocDetail2/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/LoadDocDetail2
        [ValidateAntiForgeryToken]
        [HttpPost("LoadDocDetail2", Name = "LoadDocDetail2")]
        public IActionResult Post([FromBody] DocDetailSearchModel value)
        {
            if (value == null) return null;
             
            // Use System.Text.Json to serialize the object
            var mLog = new logCreateModel { module = "DOCUMENT2(DETAIL)", tevent = "SEARCH", ref_id = 0, data_log = JsonSerializer.Serialize(value) };

            // Insert log
            //logService.insertLog(mLog);

            // Call service method  
            searchDocCreateServices service = new searchDocCreateServices();
            object result = service.SearchDetail2(value);

            // Serialize the result to JSON
            var json = JsonSerializer.Serialize(result);
              
            return Ok(json);
        }

        // PUT: api/LoadDocDetail2/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/LoadDocDetail2/5
        public void Delete(int id)
        {
        }
    }
}
