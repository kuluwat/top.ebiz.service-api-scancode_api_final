using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using top.ebiz.service.Models.Traveler_Profile;
//using System.Web.Http;
//using System.Web.Script.Serialization;
//using ebiz.webservice.Models;

namespace top.ebiz.service.Controllers.Traveler_Profile
{
    // [ApiController]
    //[Route("api/[controller]")]
    public class TrackingController : ControllerBase
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
        [HttpPost("Tracking", Name = "Tracking")]
        public IActionResult Post([FromBody] TrackingModel value)
        {
            if (value == null) return null;


            HttpResponseMessage response = null;

            string json = null;

            return Ok();
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
