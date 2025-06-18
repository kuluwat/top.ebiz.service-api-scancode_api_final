using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using top.ebiz.service.Service.Create_Trip;
using top.ebiz.service.Models.Create_Trip;
namespace top.ebiz.service.Controllers.Create_Trip
{
    public class WBSController : ControllerBase
    {
        // GET: api/WBS
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/WBS/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/WBS
        [ValidateAntiForgeryToken]
        [HttpPost("WBS", Name = "WBS")]
        public IActionResult Post([FromBody] WBSInputModel value)
        {
            if (value == null) return null;

            //logModel mLog = new logModel();
            //mLog.module = "EMPLOYEE";
            //mLog.tevent = "SEARCH";
            //mLog.ref_id = 0;
            //mLog.data_log = JsonSerializer.Serialize(value);
            ////logService.insertLog(mLog);

            HttpResponseMessage response = null;
            masterService service = new masterService();
            object result = service.getWBS(value);

            // Serialize the result to JSON
            var json = JsonSerializer.Serialize(result);
            return Ok(json);
        }

        // PUT: api/WBS/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/WBS/5
        public void Delete(int id)
        {
        }
    }
}
