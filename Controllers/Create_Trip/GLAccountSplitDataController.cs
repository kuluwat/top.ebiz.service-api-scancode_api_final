using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using top.ebiz.service.Service.Create_Trip;
using top.ebiz.service.Models.Create_Trip;

namespace top.ebiz.service.Controllers.Create_Trip
{
    public class GLAccountSplitDataController : ControllerBase
    {
        // GET: api/GLAccount
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/GLAccount/5
        public string Get(int id)
        {
            return "value";
        }

        //// POST: api/GLAccountSplitData
        //[ValidateAntiForgeryToken]
        //[HttpPost("GLAccountSplitData", Name = "GLAccountSplitData")]
        //public IActionResult Post()
        //{ 
        //    HttpResponseMessage response = null;
        //    searchDocCreateServices service = new searchDocCreateServices();
        //    var result = service.MappingSplitDataMasterGL();

        //    // Serialize the result to JSON
        //    var json = JsonSerializer.Serialize(result);
        //    return Ok(json);
        //}

        // PUT: api/GLAccount/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/GLAccount/5
        public void Delete(int id)
        {
        }
    }
}
