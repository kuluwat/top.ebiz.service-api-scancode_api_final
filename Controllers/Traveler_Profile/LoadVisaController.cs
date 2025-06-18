using System;
using System.Collections.Generic;
//using System.Data.OracleClient;
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
    public class LoadVisaController : ControllerBase
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
        [HttpPost("SearchVisa", Name = "SearchVisa")]
        public IActionResult Post([FromBody] VisaModel value)
        {
            if (value == null) return null;

            //loginClientModel loginclient = new loginClientModel();
            //loginclient.user = "ZKUL-UWAT";
            //loginclient.pass = "initial0;"; 
            //userAuthenService usau = new userAuthenService();
            //usau.loginWeb(loginclient);


            searchDocTravelerProfileServices service = new searchDocTravelerProfileServices();
            logService.logModel mLog = new logService.logModel();

            mLog.module = "Visa";
            mLog.tevent = "SearchVisa";
            mLog.ref_id = 0;
            mLog.data_log = JsonSerializer.Serialize(value);
            //logService.insertLog(mLog);

            HttpResponseMessage response = null;
            Object result = service.SearchVisa(value);
            string json = JsonSerializer.Serialize(result);


            return Ok(json);
        }
        // POST: api/Controller name
        [ValidateAntiForgeryToken]
        [HttpPost("LoadVisa", Name = "LoadVisa")]
        public IActionResult LoadVisa([FromBody] VisaModel value)
        {
            if (value == null) return null;

            //loginClientModel loginclient = new loginClientModel();
            //loginclient.user = "ZKUL-UWAT";
            //loginclient.pass = "initial0;"; 
            //userAuthenService usau = new userAuthenService();
            //usau.loginWeb(loginclient);


            searchDocTravelerProfileServices service = new searchDocTravelerProfileServices();
            logService.logModel mLog = new logService.logModel();

            mLog.module = "Visa";
            mLog.tevent = "SearchVisa";
            mLog.ref_id = 0;
            mLog.data_log = JsonSerializer.Serialize(value);
            //logService.insertLog(mLog);

            HttpResponseMessage response = null;
            Object result = service.SearchVisa(value);
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

        //https://www.taithienbo.com/connect-to-oracle-database-from-net-core-application/
    }
}
