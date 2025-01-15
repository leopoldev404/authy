using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Auth;

internal sealed class TokenProvider(IOptions<JwtSettings> settings)
{
    public string Generate(string userId, string username)
    {
        SigningCredentials credentials = new(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Value.Key)),
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
            Expires = DateTime.UtcNow.AddMinutes(settings.Value.ExpirationInMinutes),
            SigningCredentials = credentials,
            Issuer = settings.Value.Issuer,
            Audience = settings.Value.Audience,
        };

        return new JsonWebTokenHandler().CreateToken(tokenDescriptor);
    }
}
