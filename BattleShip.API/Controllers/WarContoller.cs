using System.Security.Claims;
using BattleShip.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

[Route("war")]
[ApiController]
public class WarController : Controller
{
    private readonly WarService _warService;

    public WarController(WarService warService)
    {
        _warService = warService;
    }

    [Authorize]
    [HttpPost]
    public Results<UnauthorizedHttpResult, JsonHttpResult<WarDto>> CreateWar()
    {
        ClaimsIdentity? identity = HttpContext.User.Identity as ClaimsIdentity;
        Guid? commanderId = new Guid(identity!.FindFirst("id")!.Value);
        if (identity is null || !commanderId.HasValue)
        {
            return TypedResults.Unauthorized();
        }

        War war = new War(commanderId.Value);
        _warService.Wars.Add(war.Id, war);
        // return game.PlayerGrid.ToJaggedArray();
        return TypedResults.Json(war.ToDto());
    }

    [Authorize]
    [Route("{warId}")]
    [HttpGet]
    public Results<UnauthorizedHttpResult, NotFound, JsonHttpResult<WarDto>> GetWar(Guid warId)
    {
        ClaimsIdentity? identity = HttpContext.User.Identity as ClaimsIdentity;
        Guid? commanderId = new Guid(identity!.FindFirst("id")!.Value);
        if (identity is null || !commanderId.HasValue)
        {
            return TypedResults.Unauthorized();
        }

        War? war = null;
        if (
            !_warService.Wars.ContainsKey(warId)
            || !_warService.Wars.TryGetValue(warId, out war)
            || war is null
        )
        {
            return TypedResults.NotFound();
        }

        if (
            !war.CommanderId.Equals(commanderId.Value)
            && (war.CosmosId is null || !war.CosmosId.Equals(commanderId.Value))
        )
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Json(war.ToDto());
    }

    [Authorize]
    [Route("join/{warId}")]
    [HttpPost]
    public Results<UnauthorizedHttpResult, NotFound, BadRequest, Ok> JoinWar(Guid warId)
    {
        ClaimsIdentity? identity = HttpContext.User.Identity as ClaimsIdentity;
        Guid? commanderId = new Guid(identity!.FindFirst("id")!.Value);
        if (identity is null || !commanderId.HasValue)
        {
            return TypedResults.Unauthorized();
        }

        War? war = null;
        if (
            !_warService.Wars.ContainsKey(warId)
            || !_warService.Wars.TryGetValue(warId, out war)
            || war is null
        )
        {
            return TypedResults.NotFound();
        }

        if (war.CommanderId.Equals(commanderId) || war.CosmosId != null || war.Status != WarStatus.LOBBY)
        {
            return TypedResults.BadRequest();
        }

        war.CosmosId = commanderId;

        return TypedResults.Ok();
    }

    [Authorize]
    [Route("move/{warId}")]
    [HttpPost]
    public Results<UnauthorizedHttpResult, NotFound, BadRequest, Ok> MoveSpacecraft(Guid warId, [FromBody] SpacecraftDto spacecraftDto)
    {
        ClaimsIdentity? identity = HttpContext.User.Identity as ClaimsIdentity;
        Guid? commanderId = new Guid(identity!.FindFirst("id")!.Value);
        if (identity is null || !commanderId.HasValue)
        {
            return TypedResults.Unauthorized();
        }

        War? war = null;
        if (
            !_warService.Wars.ContainsKey(warId)
            || !_warService.Wars.TryGetValue(warId, out war)
            || war is null
        )
        {
            return TypedResults.NotFound();
        }

        if (
            !war.CommanderId.Equals(commanderId.Value)
            && (war.CosmosId is null || !war.CosmosId.Equals(commanderId.Value))
        )
        {
            return TypedResults.NotFound();
        }

        if (war.Status != WarStatus.LOBBY)
        {
            return TypedResults.BadRequest();
        }

        Astec astec = war.CommanderAstec;
        if (war.CosmosId.Equals(commanderId.Value))
        {
            astec = war.CosmosAstec;
        }

        if (astec.MoveSpacecraft(new Spacecraft(spacecraftDto)))
        {
            return TypedResults.Ok();
        }

        return TypedResults.BadRequest();
    }

    [Authorize]
    [Route("start/{warId}")]
    [HttpPost]
    public Results<UnauthorizedHttpResult, NotFound, BadRequest, Ok> StartWar(Guid warId)
    {
        ClaimsIdentity? identity = HttpContext.User.Identity as ClaimsIdentity;
        Guid? commanderId = new Guid(identity!.FindFirst("id")!.Value);
        if (identity is null || !commanderId.HasValue)
        {
            return TypedResults.Unauthorized();
        }

        War? war = null;
        if (
            !_warService.Wars.ContainsKey(warId)
            || !_warService.Wars.TryGetValue(warId, out war)
            || war is null
            || !war.CommanderId.Equals(commanderId.Value)
        )
        {
            return TypedResults.NotFound();
        }

        // Only usable for AI War
        if (war.Status != WarStatus.LOBBY || war.CosmosId is not null)
        {
            return TypedResults.BadRequest();
        }

        war.Status = WarStatus.ONGOING;

        return TypedResults.Ok();
    }

    [Authorize]
    [Route("beam/{warId}")]
    [HttpPost]
    public Results<UnauthorizedHttpResult, NotFound, JsonHttpResult<BeamResponseDto>> BeamWar(Guid warId, [FromBody] BeamActionDto beamAction)
    {
        ClaimsIdentity? identity = HttpContext.User.Identity as ClaimsIdentity;
        Guid? commanderId = new Guid(identity!.FindFirst("id")!.Value);
        if (identity is null || !commanderId.HasValue)
        {
            return TypedResults.Unauthorized();
        }

        War? war = null;
        if (
            !_warService.Wars.ContainsKey(warId)
            || !_warService.Wars.TryGetValue(warId, out war)
            || war is null
        )
        {
            return TypedResults.NotFound();
        }

        if (
            !war.CommanderId.Equals(commanderId.Value)
            && (war.CosmosId is null || !war.CosmosId.Equals(commanderId.Value))
        )
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Json(war.Beam(beamAction));
    }
}
