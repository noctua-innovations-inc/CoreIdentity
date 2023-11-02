using AspNetIdentity.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AspNetIdentity.Data;

public class JsonWebToken
{
    public const string JwtAudienceEntry = "JWT:Audience";
    public const string JwtIssuerEntry = "JWT:Issuer";
    public const string JwtSecretEntry = "JWT:Secret";
    public const string JwtTtlEntry = "JWT:TTL";
    public const string JwtIdleTimeoutEntry = "JWT:MaxIdleMinutes";

    public JsonWebToken(IConfiguration configuration)
    {
        Configuration = configuration;
        SecurityTokenHandler = new JwtSecurityTokenHandler();

        JwtAudience = Configuration[JwtAudienceEntry];
        JwtIssuer = Configuration[JwtIssuerEntry];
        JwtExpires = DateTime.UtcNow.AddHours(Convert.ToDouble(Configuration[JwtTtlEntry]));

        JwtSecret = Configuration[JwtSecretEntry];
        JwtSigningAlgorithm = SecurityAlgorithms.HmacSha256Signature;
    }

    public static class CustomClaimTypes
    {
        public const string ServiceId = nameof(ServiceId);
    }

    private IConfiguration Configuration { get; }
    private JwtSecurityTokenHandler SecurityTokenHandler { get; }

    public SecurityKey JwtSecurityKey => new SymmetricSecurityKey(Encoding.ASCII.GetBytes(JwtSecret));

    public string JwtAudience { get; set; }
    public DateTime? JwtExpires { get; set; }
    public string JwtIssuer { get; set; }
    public string JwtSecret { get; set; }
    public string JwtSigningAlgorithm { get; set; }

    public List<Claim> JwtClaims { get; } = new List<Claim>();

    public JsonWebToken SetUserName(UserLogin user)
    {
        JwtClaims.Add(new Claim(ClaimTypes.Name, user.UserName));
        return this;
    }

    public JsonWebToken SetUserRoles(IList<string> roles)
    {
        if (roles == null)
        {
            return this;
        }
        foreach (var role in roles)
        {
            JwtClaims.Add(new Claim(ClaimTypes.Role, role));
        }
        return this;
    }

    public string GenerateToken()
    {
        //// Examples of Claims
        //new Claim(ClaimTypes.Name, user.UserName),
        //new Claim(ClaimTypes.Role, userRole);
        //new Claim(JwtRegisteredClaimNames.Sub, username),
        //new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        //new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(now).ToString(), ClaimValueTypes.Integer64),
        //new Claim(JwtRegisteredClaimNames.Email, "sergey.smirnov@logicblox.com", ClaimValueTypes.String)
        //new Claim(JwtRegisteredClaimNames.Birthdate, user.Birthdate.ToString("yyyy-MM-dd"))

        var jwt = new JwtSecurityToken(

            // iss (issuer), identifies the issuer of the JWT. It doesn’t matter exactly what this string is
            // (UUID, domain name, URL or something else) as long as the issuer and consumer of the JWT agree
            // on valid values, and that the consumer validates the claim matches a known good value.
            issuer: JwtIssuer,

            // aud (audience), identifies the audience of the token, that is, who should be consuming it.
            // aud may be a scalar or an array value. Again, the issuer and the consumer of the JWT should
            // agree on the specific values considered acceptable.
            audience: JwtAudience,

            claims: JwtClaims,

            // nbf (notBefore), can be useful if you are issuing a token for future use.
            notBefore: DateTime.UtcNow,

            // exp (expires), defines a time beyond which the JWT is no longer valid, and it should always be set.
            expires: JwtExpires,

            // The signature of a JWT is critical, because it guarantees the integrity of the payload and the header.
            // Verifying the signature must be the first step that any consumer of a JWT performs.
            // If the signature doesn’t match, no further processing should take place.
            signingCredentials: new SigningCredentials(JwtSecurityKey, JwtSigningAlgorithm)
        );

        var encodedJwt = SecurityTokenHandler.WriteToken(jwt);
        return encodedJwt;
    }

    public bool ValidateToken(string token, out SecurityToken validatedToken, bool shouldThrowException = false)
    {
        var result = true;
        try
        {
            _ = SecurityTokenHandler.ValidateToken(
                token,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = (!string.IsNullOrWhiteSpace(JwtAudience)),
                    ValidAudience = JwtAudience,
                    IssuerSigningKey = JwtSecurityKey
                },
                out validatedToken);
        }
        catch
        {
            if (shouldThrowException)
            {
                throw;
            }
            validatedToken = null;
            result = false;
        }
        return result;
    }
}