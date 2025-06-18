using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using top.ebiz.service.Service.Create_Trip;
using top.ebiz.service.Models.Create_Trip;

namespace top.ebiz.service.Controllers.Create_Trip
{ 
    public class LoadDocListController : ControllerBase
    { 
        // GET: api/LoadDocList
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/LoadDocList/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/LoadDocList
        [ValidateAntiForgeryToken]
        [HttpPost("LoadDocList", Name = "LoadDocList")]
        public IActionResult Post([FromBody] SearchDocumentModel value)
        {
            if (value == null) return null;
              
            // Use System.Text.Json to serialize the object
            var mLog = new logCreateModel { module = "DOCUMENT(LIST)", tevent = "SEARCH", ref_id = 0, data_log = JsonSerializer.Serialize(value) };

            // Insert log
            //logService.insertLog(mLog);

            // Call service method  
            searchDocCreateServices service = new searchDocCreateServices();
            object result = service.SearchDocuments(value);

            // Serialize the result to JSON
            var json = JsonSerializer.Serialize(result);


            return Ok(json);
        }

        // PUT: api/LoadDocList/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/LoadDocList/5
        public void Delete(int id)
        {
        }
    }
}
