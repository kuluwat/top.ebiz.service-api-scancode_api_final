using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using top.ebiz.service.Service.Create_Trip;
using top.ebiz.service.Models.Create_Trip;

namespace top.ebiz.service.Controllers.Create_Trip
{ 
    public class TravelerSummaryController : ControllerBase
    { 
        // GET: api/TravelerSummary
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // POST: api/TravelerSummary
        [ValidateAntiForgeryToken]
        [HttpPost("TravelerSummary", Name = "TravelerSummary")]
        public IActionResult Post([FromBody] TravelerSummaryModel value)
        {
            HttpResponseMessage response = null;

            logCreateModel mLog = new logCreateModel();
            mLog.module = "TAB2";
            mLog.tevent = "APPROVER";
            mLog.ref_id = 0;
            mLog.data_log = JsonSerializer.Serialize(value);
            //logService.insertLog(mLog);

            TravelerSummaryService service = new TravelerSummaryService();
            object result = service.getResult(value);

            // Serialize the result to JSON
            var json = JsonSerializer.Serialize(result);
            return Ok(json);
        }

    }
}
