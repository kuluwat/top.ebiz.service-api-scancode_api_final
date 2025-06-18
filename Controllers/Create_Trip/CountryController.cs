using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using top.ebiz.service.Service.Create_Trip;
using top.ebiz.service.Models.Create_Trip;

namespace top.ebiz.service.Controllers.Create_Trip
{ 
    public class CountryController : ControllerBase
    { 
        // GET: api/Country
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Country/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Country 
        [ValidateAntiForgeryToken]
        [HttpPost("Country", Name = "Country")]
        public IActionResult Post([FromBody] CountryModel value)
        {
            if (value == null) return null;

            logCreateModel mLog = new logCreateModel();
            mLog.module = "COUNTRY";
            mLog.tevent = "GET";
            mLog.ref_id = 0;
            mLog.data_log = JsonSerializer.Serialize(value);
            //logService.insertLog(mLog);


            HttpResponseMessage response = null;
            masterService service = new masterService();
            object result = service.getCountry(value);

            // Serialize the result to JSON
            var json = JsonSerializer.Serialize(result);
            return Ok(json);
        }

        // PUT: api/Country/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/Country/5
        public void Delete(int id)
        {
        }
    }
}
