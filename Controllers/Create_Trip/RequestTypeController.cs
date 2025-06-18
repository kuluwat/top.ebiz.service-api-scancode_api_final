using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using top.ebiz.service.Service.Create_Trip;
using top.ebiz.service.Models.Create_Trip;

namespace top.ebiz.service.Controllers.Create_Trip
{ 
    public class RequestTypeController : ControllerBase
    { 
        // GET: api/RequestType
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/RequestType/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/RequestType
        [ValidateAntiForgeryToken]
        [HttpPost("RequestType", Name = "RequestType")]
        public IActionResult Post([FromBody] RequestTypeModel value)
        {
            if (value == null) return null;
             
            //logModel mLog = new logModel();
            //mLog.module = "REQUEST_TYPE";
            //mLog.tevent = "GET";
            //mLog.ref_id = 0;
            //mLog.data_log = JsonSerializer.Serialize(value);
            ////logService.insertLog(mLog);

            HttpResponseMessage response = null;
            masterService service = new masterService();
            object result = service.getRequestType(value);

            // Serialize the result to JSON
            var json = JsonSerializer.Serialize(result);
            return Ok(json);
        }

        // PUT: api/RequestType/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/RequestType/5
        public void Delete(int id)
        {
        }
    }
}
