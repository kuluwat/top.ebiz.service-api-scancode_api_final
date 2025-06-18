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
using top.ebiz.service.Service.Traveler_Profile;

namespace top.ebiz.service.Controllers.Traveler_Profile
{
    // [ApiController]
    //[Route("api/[controller]")]
    public class SaveMasterDataController : ControllerBase
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
        [HttpPost("SaveMasterData", Name = "SaveMasterData")]
        public IActionResult Post([FromBody] MMaintainDataModel value)
        {
            if (value == null) return null;

            var page_name = value.page_name;//airticket,accommodation
            var module_name = value.module_name;


            SetMasterDataService service = new SetMasterDataService();
            logService.logModel mLog = new logService.logModel();

            mLog.module = "Maintain Data";
            mLog.tevent = "Save Master Data";
            mLog.ref_id = 0;
            mLog.data_log = JsonSerializer.Serialize(value);
            //logService.insertLog(mLog);

            HttpResponseMessage response = null;
            Object result = null;
            if (module_name == "master airticket type")
            {
                result = service.SetAirticketType(value);
            }
            else if (module_name == "master already booked")
            {
                result = service.SetAlreadyBooked(value);
            }
            else if (module_name == "master list status" || module_name == "master book status")
            {
                result = service.SetListStatus(value);
            }
            else if (module_name == "master allowance type")
            {
                result = service.SetAllowanceType(value);
            }
            else if (module_name == "master feedback type")
            {
                result = service.SetFeedbackType(value);
            }
            else if (module_name == "master feedback list")
            {
                result = service.SetFeedbackList(value);
            }
            else if (module_name == "master feedback question")
            {
                result = service.SetFeedbackQuestion(value);
            }
            else if (module_name == "config daily allowance")
            {
                result = service.SetConfigDailyAllowance(value);
            }
            else if (module_name == "master insurance plan")
            {
                result = service.SetInsurancePlan(value);
            }
            else if (module_name == "master insurance broker")
            {
                result = service.SetInsurancebroker(value);
            }
            else if (module_name == "master visa document")
            {
                result = service.SetVISADocument(value);
            }
            else if (module_name == "master visa docountries")
            {
                result = service.SetVISADocountries(value);
            }
            else
            {

            }
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
