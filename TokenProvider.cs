using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Auth;

internal sealed class TokenProvider(IConfiguration configuration)
{
    public string Generate(string userId, string username)
    {
        SigningCredentials credentials = new(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
            SecurityAlgorithms.HmacSha512
        );

        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Subject = new ClaimsIdentity(
                [
                    new Claim(JwtRegisteredClaimNames.Sub, userId),
                    new Claim(JwtRegisteredClaimNames.Name, username),
                ]
            ),
            Expires = DateTime.UtcNow.AddMinutes(
                int.Parse(configuration["Jwt:ExpirationInMinutes"]!, null)
            ),
            SigningCredentials = credentials,
            Issuer = configuration["Jwt:Issuer"]!,
            Audience = configuration["Jwt:Audience"]!,
        };

        return new JsonWebTokenHandler().CreateToken(tokenDescriptor);
    }
}
