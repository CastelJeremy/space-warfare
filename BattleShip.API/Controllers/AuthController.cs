using System.Security.Cryptography;
using System.Text;
using BattleShip.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

[Route("auth")]
[ApiController]
public class AuthController : Controller
{
    private readonly AuthContext _authContext;
    private readonly JwtService _jwtService;

    public AuthController(AuthContext authContext, JwtService jwtService)
    {
        _authContext = authContext;
        _jwtService = jwtService;
    }

    [Route("register")]
    [HttpPost]
    public Results<BadRequest, JsonHttpResult<TokenDto>> RegisterCommander(CommanderRegisterDto commanderRegister)
    {
        var query = _authContext.Commanders.Where(c => c.Username.Equals(commanderRegister.Username));
        if (!query.IsNullOrEmpty())
        {
            return TypedResults.BadRequest();
        }

        SHA3_256 sha256 = SHA3_256.Create();
        string hashedPassword = Convert.ToHexString(sha256.ComputeHash(Encoding.UTF8.GetBytes(commanderRegister.Password)));

        Commander commander = new Commander { Username = commanderRegister.Username, Password = hashedPassword, Score = 0 };
        _authContext.Commanders.Add(commander);
        _authContext.SaveChanges();

        string token = _jwtService.GenerateToken(commander);

        Session session = new Session
        {
            CommanderId = commander.Id,
            CreationDate = DateTime.UtcNow,
            Ip = Request.HttpContext.Connection.RemoteIpAddress!.ToString(),
            Fingerprint = Request.Headers["User-Agent"]!
        };

        _authContext.Sessions.Add(session);
        _authContext.SaveChanges();

        return TypedResults.Json(new TokenDto { AccessToken = token });
    }

    [Route("login")]
    [HttpPost]
    public Results<BadRequest, JsonHttpResult<TokenDto>> LoginCommander(CommanderLoginDto commanderLogin)
    {
        var query = _authContext.Commanders.Where(c => c.Username.Equals(commanderLogin.Username));
        if (query.IsNullOrEmpty())
        {
            return TypedResults.BadRequest();
        }

        Commander commander = query.First();
        SHA3_256 sha256 = SHA3_256.Create();
        string hashedPassword = Convert.ToHexString(sha256.ComputeHash(Encoding.UTF8.GetBytes(commanderLogin.Password)));
        if (hashedPassword != commander.Password)
        {
            return TypedResults.BadRequest();
        }

        string token = _jwtService.GenerateToken(commander);

        Session session = new Session
        {
            CommanderId = commander.Id,
            CreationDate = DateTime.UtcNow,
            Ip = HttpContext.Connection.RemoteIpAddress!.ToString(),
            Fingerprint = Request.Headers["User-Agent"]!
        };

        _authContext.Sessions.Add(session);
        _authContext.SaveChanges();

        return TypedResults.Json(new TokenDto { AccessToken = token });
    }
}
