
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using top.ebiz.Repositoires;
using top.ebiz.service.Constants.Configulations;
using top.ebiz.service.Models.Create_Trip;

namespace ebiz.webservice.service.Controllers;
public class TokenController : ControllerBase
{
    [ValidateAntiForgeryToken]
    [HttpPost("token", Name = "Token")]
    public IActionResult Token([FromForm] string userName, [FromServices] AppSettings appSettings, [FromServices] IUserAuthenRepository userAuthenRepository)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return BadRequest("Username is required.");
            }
            var user = userAuthenRepository.Login(new loginModel { user_name = userName });
            var token = JwtUtil.CreateToken(user.token_login, appSettings, new[] { new Claim("username", userName) });

            return Ok(new { access_token = token });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }


}
