using Azure.Core;
using System.IdentityModel.Tokens.Jwt;

namespace EShopApi.Services
{
    public interface IAuthService
    {
        Guid GetUserIdFromJwt(string jwt);
    }

    public class AuthService : IAuthService
    {
        public Guid GetUserIdFromJwt(string jwt)
        {            
            if (!(string.IsNullOrEmpty(jwt)))
            {
                string JwtToken = jwt.StartsWith("Bearer ") ? jwt[7..] : jwt;
                JwtSecurityTokenHandler handler = new();
                var jsonToken = handler.ReadJwtToken(JwtToken);
                string? userId2 = jsonToken.Payload["UserId"]?.ToString();
                if (Guid.TryParse(userId2, out Guid userId)) 
                    return userId;
            }

            throw new InvalidOperationException("User ID could not be extracted from the JWT.");
        }
    }
}
