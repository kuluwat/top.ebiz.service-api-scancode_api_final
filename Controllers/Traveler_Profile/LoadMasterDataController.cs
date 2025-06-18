using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using top.ebiz.service.Models.Traveler_Profile;
using top.ebiz.service.Service.Traveler_Profile;

namespace top.ebiz.service.Controllers.Traveler_Profile
{
    // [ApiController]
    //[Route("api/[controller]")]
    public class LoadMasterDataController : ControllerBase
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
        [HttpPost("LoadMasterData", Name = "LoadMasterData")]
        public IActionResult Post([FromBody] MMaintainDataModel value)
        {
            if (value == null) return null;

            var page_name = value.page_name;//airticket,accommodation
            var module_name = value.module_name;


            searchMasterDataService service = new searchMasterDataService();
            logService.logModel mLog = new logService.logModel();

            if ((page_name + "") == "")
            {
                value.page_name = "Maintain Data";
            }
            mLog.module = "Maintain Data";
            mLog.tevent = "Search Master Data";
            mLog.ref_id = 0;
            mLog.data_log = JsonSerializer.Serialize(value);
            //logService.insertLog(mLog);

            HttpResponseMessage response = null;
            Object result = null;
            if (module_name == "master airticket type" || module_name == "master already booked")
            {
                result = service.SearchAlreadyBooked(value);
            }
            else if (module_name == "master list status"
                  || module_name == "master book status")
            {
                result = service.SearchListStatus(value);
            }
            else if (module_name == "master allowance type")
            {
                result = service.SearchAllowanceType(value);
            }
            else if (module_name == "master feedback type")
            {
                result = service.SearchFeedbackType(value);
            }
            else if (module_name == "master feedback list")
            {
                result = service.SearchFeedbackList(value);
            }
            else if (module_name == "master feedback question")
            {
                result = service.SearchFeedbackQuestion(value);
            }
            else if (module_name == "config daily allowance")
            {
                result = service.SearchConfigDailyAllowance(value);
            }
            else if (module_name == "master visa document")
            {
                result = service.SearchVISADocument(value);
            }
            else if (module_name == "master visa docountries")
            {
                result = service.SearchVISADocountries(value);
            }
            else if (module_name == "master location")
            {
                result = service.SearchZoneCountryProvince(value);
            }
            else if (module_name == "master airport")
            {
                result = service.SearchAirport(value);
            }
            else if (module_name == "master insurance broker")
            {
                result = service.SearchInsurancebroker(value);
            }
            else if (module_name == "master zone country")
            {
                result = service.SearchZoneCountry(value);
            }
            else
            {
                result = service.SearchZoneCountry(value);
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

        //https://www.taithienbo.com/connect-to-oracle-database-from-net-core-application/
    }
}
