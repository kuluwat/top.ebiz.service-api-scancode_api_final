
using System.Net;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using top.ebiz.service.Service.Create_Trip;
using top.ebiz.service.Models.Create_Trip;

namespace top.ebiz.service.Controllers.Create_Trip
{ 
    public class ContinentController : ControllerBase
    { 
        // GET: api/Continent
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Continent/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Continent 
        [ValidateAntiForgeryToken]
        [HttpPost("Continent", Name = "Continent")]
        public IActionResult Post([FromBody] ContinentModel value)
        {
            if (value == null) return null;

            HttpResponseMessage response = null;
            masterService service = new masterService();
            object result = service.getContinuent(value);

            // Serialize the result to JSON
            var json = JsonSerializer.Serialize(result);
            return Ok(json);

        }

        // PUT: api/Continent/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/Continent/5
        public void Delete(int id)
        {
        }
    }
}
