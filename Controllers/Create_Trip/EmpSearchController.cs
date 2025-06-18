using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using top.ebiz.service.Service.Create_Trip;
using top.ebiz.service.Models.Create_Trip;


namespace top.ebiz.service.Controllers.Create_Trip
{ 
    public class EmpSearchController : ControllerBase
    { 

        // GET: api/EmpSearch
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/EmpSearch/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/EmpSearch
        [ValidateAntiForgeryToken]
        [HttpPost("EmpSearch", Name = "EmpSearch")]
        public IActionResult Post([FromBody] EmpSearchModel value)
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
            object result = service.getEmployee(value);

            // Serialize the result to JSON
            var json = JsonSerializer.Serialize(result);
            return Ok(json);
        }

        // PUT: api/EmpSearch/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/EmpSearch/5
        public void Delete(int id)
        {
        }
    }
}
