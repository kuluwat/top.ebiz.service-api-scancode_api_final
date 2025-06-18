using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using top.ebiz.service.Service.Create_Trip;
using top.ebiz.service.Models.Create_Trip;

namespace top.ebiz.service.Controllers.Create_Trip
{ 
    public class docFlow4Controller : ControllerBase
    { 
        // GET: api/docFlow4
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/docFlow4/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/docFlow4 
        [ValidateAntiForgeryToken]
        [HttpPost("DocFlow4", Name = "DocFlow4")]
        public IActionResult Post([FromBody] DocFlow3Model value)
        {
            if (value == null) return null;
             
            logCreateModel mLog = new logCreateModel();
            mLog.module = "DOCUMENT";
            mLog.tevent = "FLOW4";
            mLog.ref_id = 0;
            mLog.data_log = JsonSerializer.Serialize(value);
            //logService.insertLog(mLog);

            HttpResponseMessage response = null;
            documentService service = new documentService();
            object result = service.submitFlow4(value);
            // Serialize the result to JSON
            var json = JsonSerializer.Serialize(result);
            return Ok(json);
        }

        // PUT: api/docFlow4/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/docFlow4/5
        public void Delete(int id)
        {
        }
    }
}
