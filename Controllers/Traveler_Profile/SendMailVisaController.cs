using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
//using System.Web.Http;
//using System.Web.Script.Serialization;
using System.Text.Json;
using top.ebiz.service.Models.Traveler_Profile;
using top.ebiz.service.Service.Traveler_Profile;
using Microsoft.AspNetCore.Mvc;

namespace top.ebiz.service.Controllers.Traveler_Profile
{
    // [ApiController]
    //[Route("api/[controller]")]
    public class SendMailVisaController : ControllerBase
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
        [HttpPost("SendMailVisa", Name = "SendMailVisa")]
        public IActionResult Post([FromBody] VisaOutModel value)
        {

            if (value == null) return null;


            SetDocService service = new SetDocService();
            logService.logModel mLog = new logService.logModel();

            mLog.module = "Visa";
            mLog.tevent = "SendMailVisa";
            mLog.ref_id = 0;
            mLog.data_log = JsonSerializer.Serialize(value);
            //logService.insertLog(mLog);

            HttpResponseMessage response = null;
            Object result = service.SendMailVisa(value);
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
