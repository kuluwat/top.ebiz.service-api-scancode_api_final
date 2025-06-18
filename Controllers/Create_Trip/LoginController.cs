
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using top.ebiz.service.Models.Create_Trip;
using top.ebiz.service.Service.Create_Trip;


namespace top.ebiz.service.Controllers.Create_Trip
{
    public class LoginController : ControllerBase
    {
        [ValidateAntiForgeryToken]
        [HttpPost("login", Name = "login")]
        public IActionResult Post([FromBody] loginModel value)
        {
            if (value == null) return null;

            logCreateModel mLog = new logCreateModel();
            mLog.module = "login";
            mLog.tevent = "";
            mLog.ref_id = 0;
            mLog.data_log = JsonSerializer.Serialize(value);
            //logService.insertLog(mLog);

            HttpResponseMessage response = null;
            userAuthenService service = new userAuthenService();
            object result = service.login(value);

            // Serialize the result to JSON
            var json = JsonSerializer.Serialize(result);
            return Ok(json);

        }

    }
}
