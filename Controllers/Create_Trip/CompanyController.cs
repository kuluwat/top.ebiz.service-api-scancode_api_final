

using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using top.ebiz.service.Service.Create_Trip;
using top.ebiz.service.Models.Create_Trip;

namespace top.ebiz.service.Controllers.Create_Trip
{ 
    public class CompanyController : ControllerBase
    { 

        // GET: api/Company
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Company/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Company 
        [ValidateAntiForgeryToken]
        [HttpPost("Company", Name = "Company")]
        public IActionResult Post([FromBody] CompanyModel value)
        {
            if (value == null) return null;

            logCreateModel mLog = new logCreateModel();
            mLog.module = "COMPANY";
            mLog.tevent = "GET";
            mLog.ref_id = 0;
            mLog.data_log = JsonSerializer.Serialize(value);
            //logService.insertLog(mLog);
              
            HttpResponseMessage response = null;
            masterService service = new masterService();
            object result = service.getCompany(value);

            // Serialize the result to JSON
            var json = JsonSerializer.Serialize(result);
            return Ok(json);
        }

        // PUT: api/Company/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/Company/5
        public void Delete(int id)
        {
        }
    }
}
