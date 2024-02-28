using System.Security.Claims;
using BattleShip.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

[Route("sessions")]
[ApiController]
public class SessionController : Controller
{
    private readonly AuthContext _authContext;

    public SessionController(AuthContext authContext)
    {
        _authContext = authContext;
    }

    [Authorize]
    [HttpGet]
    public Results<UnauthorizedHttpResult, JsonHttpResult<SessionDto[]>> GetSessions()
    {
        ClaimsIdentity? identity = HttpContext.User.Identity as ClaimsIdentity;
        Guid? commanderId = new Guid(identity!.FindFirst("id")!.Value);
        if (identity is null || commanderId is null)
        {
            return TypedResults.Unauthorized();
        }

        SessionDto[] sessions = [];
        var query = _authContext.Sessions.Where(s => s.CommanderId.Equals(commanderId));
        if (!query.IsNullOrEmpty())
        {
            sessions = query.Select((s) => new SessionDto
            {
                Id = s.Id,
                CreationDate = s.CreationDate,
                Ip = s.Ip,
                Fingerprint = s.Fingerprint
            }).ToArray();
        }

        return TypedResults.Json(sessions);
    }

    [Authorize]
    [HttpDelete]
    [Route("{sessionId}")]
    public Results<UnauthorizedHttpResult, NotFound, Ok> DeleteSession(Guid sessionId)
    {
        ClaimsIdentity? identity = HttpContext.User.Identity as ClaimsIdentity;
        Guid? commanderId = new Guid(identity!.FindFirst("id")!.Value);
        if (identity is null || commanderId is null)
        {
            return TypedResults.Unauthorized();
        }

        var query = _authContext.Sessions.Where(s => s.Id.Equals(sessionId) && s.CommanderId.Equals(commanderId));
        if (query.IsNullOrEmpty())
        {
            return TypedResults.NotFound();
        }

        Session session = query.First();
        _authContext.Sessions.Remove(session);
        _authContext.SaveChanges();

        return TypedResults.Ok();

    }
}
