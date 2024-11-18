using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Markcons.Auth
{
    public static class JwtAuthentication
    {
        public static string GenerateJwtToken(string username)
        {
            var claims = new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, username),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("dGhpcyBpcyBhIHNlY3JldCBrZXkgdGhhdCBpcyBzZWN1cmUgZm9yIGp3dCB0b2tlbnMgYW5kIG1ha2VzIGl0IGhpZ2hseSB1bmJyaWFuZC4="));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
                issuer: "Markcons",
                audience: "CatosComputerClub",
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
