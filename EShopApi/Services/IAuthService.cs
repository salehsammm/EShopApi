using Azure.Core;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EShopApi.Services
{
    public interface IAuthService
    {
        string GenerateJwt(Guid userId, string userName);
    }

    public class AuthService(IConfiguration config) : IAuthService
    {
        public string GenerateJwt(Guid userId, string userName)
        {
            List<Claim> claims =
            [
                new Claim("UserId",userId.ToString()),
                new Claim("UserName",userName)
            ];

            SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes((config["Jwt:Key"]) ?? ""));
            SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(4),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
   
    }
}
