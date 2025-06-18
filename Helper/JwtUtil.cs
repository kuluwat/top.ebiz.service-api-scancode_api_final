using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using top.ebiz.service.Constants.Configulations;

public class JwtUtil
{

    public static string CreateToken(string? userId, AppSettings appSettings, IEnumerable<Claim>? Claims = null)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(appSettings.JwtConfigs.SecretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId ?? ""),
                // new Claim(ClaimTypes.Role, Role.CONTACT_ADMIN.ToString()),
                // new Claim(ClaimTypes.Role, Role.PMSV_ADMIN.ToString()),
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            Audience= appSettings.JwtConfigs.Audience,
            Issuer = appSettings.JwtConfigs.Issuer,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        if (Claims is not null && Claims.Any())
        {
            tokenDescriptor.Subject.AddClaims(Claims);
        }
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public ClaimsPrincipal? ValidateToken(string token, AppSettings appSettings)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(appSettings.JwtConfigs.SecretKey);
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidateAudience = true
        };

        try
        {
            return tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
        }
        catch
        {
            return null;
        }
    }

    public T? ConvertPayload<T>(ClaimsPrincipal? principal) where T : class
    {
        if (principal == null)
        {
            return null;
        }

        var jwtToken = principal.Identity as JwtSecurityToken;
        if (jwtToken == null)
        {
            return null;
        }

        var payload = jwtToken.Payload.SerializeToJson();
        return JsonConvert.DeserializeObject<T>(payload);
    }


}