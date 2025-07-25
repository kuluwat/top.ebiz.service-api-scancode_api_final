﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;


using Microsoft.AspNetCore.Mvc;
using top.ebiz.service.Models.Traveler_Profile;
using top.ebiz.service.Service.Traveler_Profile;

namespace top.ebiz.service.Controllers.Traveler_Profile
{
    // [ApiController]
    //[Route("api/[controller]")]
    public class LoadAllowanceMasterController : ControllerBase
    {
        // GET: api/Controller name

        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Controller name/5

        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Controller name
        [ValidateAntiForgeryToken]
        [HttpPost("LoadAllowanceMaster", Name = "LoadAllowanceMaster")]
        public IActionResult Post([FromBody] AllowanceModel value)
        {
            if (value == null) return null;

            searchDocTravelerProfileServices service = new searchDocTravelerProfileServices();
            logService.logModel mLog = new logService.logModel();

            mLog.module = "AllowanceMaster";
            mLog.tevent = "SearchAllowanceMaster";
            mLog.ref_id = 0;
            mLog.data_log = JsonSerializer.Serialize(value);
            //logService.insertLog(mLog);

            HttpResponseMessage response = null;
            Object result = service.SearchAllowanceMaster(value);
            string json = JsonSerializer.Serialize(result);

            return Ok(json);
        }

        // PUT: api/Controller name/5

        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/Controller name/5

        public void Delete(int id)
        {
        }



    }
}
