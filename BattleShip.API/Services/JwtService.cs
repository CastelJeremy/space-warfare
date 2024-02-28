using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BattleShip.Models;
using System.Text;

public class JwtService
{
    private string _issuer;
    private string _audience;
    private SymmetricSecurityKey _securityKey;

    public JwtService()
    {
        _issuer = "http://mysite.com";
        _audience = "http://myaudience.com";
        _securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("asdv234234^&%&^%&^hjsdfb2%%%123456789"));
    }

    public string GenerateToken(Commander commander)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[] { new Claim("id", commander.Id.ToString()) }),
            Expires = DateTime.UtcNow.AddDays(3),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
