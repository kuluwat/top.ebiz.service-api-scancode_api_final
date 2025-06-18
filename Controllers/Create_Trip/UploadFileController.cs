
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using top.ebiz.service.Models.Create_Trip;
using top.ebiz.service.Service.Create_Trip;

namespace top.ebiz.service.Controllers.Create_Trip
{
    public class UploadFileController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UploadFileController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // POST: api/UploadFile
        //20250327 0000 log scan => echnical Impact: Gain privileges     CWE 352: Cross-Site Request Forgery(CSRF) 
        [ValidateAntiForgeryToken] 
        [HttpPost("UploadFile", Name = "UploadFile")]
        public IActionResult Post()
        {
            logCreateModel mLog = new logCreateModel();
            mLog.module = "UploadFileTravelerhistory";
            mLog.tevent = "";
            mLog.ref_id = 0;
            //logService.insertLog(mLog);

            var files = Request.Form.Files;
            var fileDoc = Request.Form["file_doc"];
            var fileTokenLogin = Request.Form["file_token_login"];
             
            HttpResponseMessage response = null;
            documentService service = new documentService();
            object result = service.uploadfile_travelerhistory(_httpContextAccessor);

            // Serialize the result to JSON
            var json = JsonSerializer.Serialize(result);
            return Ok(json);
        }

    }
}
