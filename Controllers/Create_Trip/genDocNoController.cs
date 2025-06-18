using Microsoft.AspNetCore.Mvc;
using top.ebiz.service.Service.Create_Trip;
using top.ebiz.service.Models.Create_Trip;
using System.Text.Json;


namespace top.ebiz.service.Controllers.Create_Trip
{ 
    public class genDocNoController : ControllerBase
    {  
        // GET: api/genDocNo
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/genDocNo/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/genDocNo
        [ValidateAntiForgeryToken]
        [HttpPost("genDocNo", Name = "genDocNo")]
        public IActionResult Post([FromBody] genDocNoModel value)
        {
            if (value == null) return null;
              
            // Use System.Text.Json to serialize the object
            var mLog = new logCreateModel { module = "DOCUMENT", tevent = "GEN_DOCNO", ref_id = 0, data_log = JsonSerializer.Serialize(value) };

            // Insert log
            //logService.insertLog(mLog);

            // Call service method 
            documentService service = new documentService();
            object result = service.genDocNo(value);

            // Serialize the result to JSON
            var json = JsonSerializer.Serialize(result);
            return Ok(json);
        }

        // PUT: api/genDocNo/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/genDocNo/5
        public void Delete(int id)
        {
        }
    }
}
