using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

using System.Text.Json;
using top.ebiz.service.Models.Traveler_Profile;
using top.ebiz.service.Service.Traveler_Profile;

namespace top.ebiz.service.Controllers.Traveler_Profile
{
    // [ApiController]
    //[Route("api/[controller]")]
    public class EmailDetailsController : ControllerBase
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
        [HttpPost("CheckIP", Name = "CheckIP")]
        public IActionResult Post([FromBody] ImgList value)
        {

            SetDocService service = new SetDocService();
            logService.logModel mLog = new logService.logModel();

            mLog.module = "Check IP";
            mLog.tevent = GetIPAddress();
            mLog.ref_id = 0;
            mLog.data_log = JsonSerializer.Serialize(value);
            //logService.insertLog(mLog);

            HttpResponseMessage response = null;
            string json = JsonSerializer.Serialize(mLog);

            return Ok(json);
        }
        protected string GetIPAddress()
        {
            //System.Web.HttpContext context = System.Web.HttpContext.Current;
            //string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            //if (!string.IsNullOrEmpty(ipAddress))
            //{
            //    string[] addresses = ipAddress.Split(',');
            //    if (addresses.Length != 0)
            //    {
            //        return addresses[0];
            //    }
            //}

            //return context.Request.ServerVariables["REMOTE_ADDR"];

            return "";
        }

    }
}
