using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Web;

namespace top.ebiz.service.Controllers.Create_Trip
{
    [AllowAnonymous]
    public class GetAntiForgeryTokenController : Controller
    {
        private readonly IAntiforgery _antiforgery;

        public GetAntiForgeryTokenController(IAntiforgery antiforgery)
        {
            _antiforgery = antiforgery;
        }

        [HttpGet("GetAntiForgeryToken", Name = "GetAntiForgeryToken")]
        public IActionResult Get()
        {
            if (HttpContext == null)
            {
                return BadRequest(new { error = "HttpContext is null" });
            }

            var tokens = _antiforgery.GetAndStoreTokens(HttpContext); // สร้าง CSRF token และเก็บใน Cookie
            return Ok(new { csrfToken = tokens.RequestToken });
        }


        [ValidateAntiForgeryToken]
        [HttpPost("GetAntiForgeryTokenCheck", Name = "GetAntiForgeryTokenCheck")]
        public string GetAntiForgeryTokenCheck([FromBody] AntiForgeryTokenModel param)
        {
            string result = param.csrfToken;
            return HttpUtility.HtmlEncode(result);
        }
        public class AntiForgeryTokenModel
        {
            public string csrfToken { get; set; } = string.Empty;
        }

    }
}
