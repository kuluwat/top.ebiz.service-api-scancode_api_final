using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
//using System.Web.Http;
//using System.Web.Script.Serialization;
using System.Text.Json;
using top.ebiz.service.Models.Traveler_Profile;
using top.ebiz.service.Service;
using top.ebiz.service.Service.Traveler_Profile;
//using ebiz.webservice.Service;

namespace top.ebiz.service.Controllers.Traveler_Profile
{
    // [ApiController]
    //[Route("api/[controller]")]
    public class SendMailTestController : ControllerBase
    {
        // GET: api/Controller name
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Controller name/5
        public string Get(int id)
        {
            return id.ToString();
        }

        // POST: api/Controller name
        //[ValidateAntiForgeryToken]
        [IgnoreAntiforgeryToken]
        [HttpPost("SendMailTestThaioilgroup", Name = "SendMailTestThaioilgroup")]
        public IActionResult Post()
        {
            ClassMail443 swemail = new ClassMail443();
             
            string ret = "";
            string msg_error = "";
            try
            {
               // ret = swemail.SendMail_Normal();
                ret = swemail.send_mail();
            }
            catch (Exception ex)
            {
                ret = "false";
                msg_error = ex.Message.ToString();
            } 
            return Ok(msg_error);
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
