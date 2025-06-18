
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using top.ebiz.service.Models.Create_Trip; 
using top.ebiz.service.Service.Create_Trip;

namespace top.ebiz.service.Controllers.Create_Trip
{ 
    public class loginWebController : ControllerBase
    { 
        // GET: api/loginWeb
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/loginWeb/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/loginWeb
        [ValidateAntiForgeryToken]
        [HttpPost("loginWeb", Name = "loginWeb")]
        public IActionResult Post([FromBody] loginClientModel value)
        {
            if (value == null) return null;

            logCreateModel mLog = new logCreateModel();
            mLog.module = "loginWeb";
            mLog.tevent = "";
            mLog.ref_id = 0;
            mLog.data_log = JsonSerializer.Serialize(value);
            //logService.insertLog(mLog);

            HttpResponseMessage response = null;
            userAuthenService service = new userAuthenService();
            object result = service.loginWeb(value);

            // Serialize the result to JSON
            var json = JsonSerializer.Serialize(result);
            return Ok(json);
        }

        // PUT: api/loginWeb/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/loginWeb/5
        public void Delete(int id)
        {
        }
    }
}
