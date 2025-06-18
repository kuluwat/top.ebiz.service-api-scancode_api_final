using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using top.ebiz.service.Service.Create_Trip;
using top.ebiz.service.Models.Create_Trip;

namespace top.ebiz.service.Controllers.Create_Trip
{ 
    public class getTravelDayController : ControllerBase
    { 
        // GET: api/getTravelDay
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/getTravelDay/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/getTravelDay
        [ValidateAntiForgeryToken]
        [HttpPost("getTravelDay", Name = "getTravelDay")]
        public IActionResult Post([FromBody] TravelDayModel value)
        {
            if (value == null) return null;
             
            //logModel mLog = new logModel();
            //mLog.module = "TRAVEL_DAY";
            //mLog.tevent = "GET";
            //mLog.ref_id = 0;
            //mLog.data_log = JsonSerializer.Serialize(value);
            ////logService.insertLog(mLog);
             
            HttpResponseMessage response = null;
            travelDayService service = new travelDayService();
            object result = service.getTravelDay(value);

            // Serialize the result to JSON
            var json = JsonSerializer.Serialize(result);
            return Ok(json);
        }

        // PUT: api/getTravelDay/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/getTravelDay/5
        public void Delete(int id)
        {
        }
    }
}
