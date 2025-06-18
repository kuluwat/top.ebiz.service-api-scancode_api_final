 
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using top.ebiz.service.Models.Traveler_Profile;
using top.ebiz.service.Service.Traveler_Profile;

namespace ebiz.webservice.service.Controllers
{
    // [ApiController]
    //[Route("api/[controller]")]
    public class EmailConfigController : ControllerBase
    {
        private readonly logService _logService; 

        public EmailConfigController(logService logService )
        {
            _logService = logService; 
        }
        // GET: api/EmailConfig

        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/EmailConfig/5

        public string Get(int id)
        {
            return "value";
        }

        // POST: api/EmailConfig
        [ValidateAntiForgeryToken]
        [HttpPost("EmailConfig", Name = "EmailConfig")]
        public IActionResult Post([FromBody] EmailModel value)
        {
            if (value == null) return null;


            logService.logModel mLog = new logService.logModel();
            mLog.module = "email";
            mLog.tevent = "";
            mLog.ref_id = 0;
            mLog.data_log = JsonSerializer.Serialize(value);
            //logService.insertLog(mLog);

            //flow : E-Biz-008 Trabelling Isurance 
            //step : 2 to 3
            //page : travelinsurance ,action : NotiTravelInsuranceForm

            //step : 3 to 4
            //page : travelinsurance ,action : NotiTravelInsuranceListPassportInfo

            //step : 4 to 5
            //page : travelinsurance ,action : NotiTravelInsuranceCertificates

            //flow : E-Biz-009 ISOS 
            //step : 3 to 4
            //page : isos ,action : NotiISOSNewListRuningNoName

            //step : 4 to 5
            //page : isos ,action : NotiISOSNewList


            // Send email service
            HttpResponseMessage response = null;
            SendEmailServiceTravelerProfile service = new SendEmailServiceTravelerProfile();
            object result = service.EmailConfig(value);

            // Serialize and return the result
            var json = JsonSerializer.Serialize(result);
            return Ok(json);
        }

        // PUT: api/EmailConfig/5

        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/EmailConfig/5

        public void Delete(int id)
        {

        }
    }
}
