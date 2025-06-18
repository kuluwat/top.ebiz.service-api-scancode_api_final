using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using top.ebiz.service.Service.Create_Trip;
using top.ebiz.service.Models.Create_Trip;

namespace top.ebiz.service.Controllers.Create_Trip
{ 
    public class EstimateExpenseController : ControllerBase
    { 
        // GET: api/EstimateExpense
        public IEnumerable<string> Get()
        {
            EstimateExpenseService service = new EstimateExpenseService();
            DateTime? d = new DateTime(2020, 3, 30);
            DateTime dchk = (DateTime)d;
            DateTime result = dchk.AddMonths(-2);
            //decimal result = service.getExpireDateBefore(d);
            return new string[] { "value1", "value2" };
        }

        // GET: api/EstimateExpense/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/EstimateExpense 
        [ValidateAntiForgeryToken]
        [HttpPost("EstimateExpense", Name = "EstimateExpense")]
        public IActionResult Post([FromBody] EstExpInputModel value)
        {
            if (value == null) return null;


            //logModel mLog = new logModel();
            //mLog.module = "COMPANY";
            //mLog.tevent = "GET";
            //mLog.ref_id = 0;
            //mLog.data_log = JsonSerializer.Serialize(value);
            ////logService.insertLog(mLog);

            HttpResponseMessage response = null;
            EstimateExpenseService service = new EstimateExpenseService();
            object result = service.EstimateExpense(value);

            // Serialize the result to JSON
            var json = JsonSerializer.Serialize(result);
            return Ok(json);
        }

        // PUT: api/EstimateExpense/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/EstimateExpense/5
        public void Delete(int id)
        {
        }
    }
}
