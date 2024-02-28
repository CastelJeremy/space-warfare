using BattleShip.Models;
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

    [HttpPost]
    public JsonHttpResult<WarDto> CreateWar()
    {
        War war = new War();
        _warService.Wars.Add(war.Id, war);
        // return game.PlayerGrid.ToJaggedArray();
        return TypedResults.Json(war.ToDto());
    }

    [Route("{warId}")]
    [HttpGet]
    public Results<NotFound, JsonHttpResult<WarDto>> GetWar(Guid warId)
    {
        if (!_warService.Wars.ContainsKey(warId))
        {
            return TypedResults.NotFound();
        }

        War war = _warService.Wars[warId];

        return TypedResults.Json(war.ToDto());
    }

    [Route("beam/{warId}")]
    [HttpPost]
    public Results<NotFound, JsonHttpResult<BeamResponseDto>> BeamWar(Guid warId, [FromBody] BeamActionDto beamAction)
    {
        if (!_warService.Wars.ContainsKey(warId))
        {
            return TypedResults.NotFound();
        }

        War war = _warService.Wars[warId];

        return TypedResults.Json(war.Beam(beamAction));
    }
}
