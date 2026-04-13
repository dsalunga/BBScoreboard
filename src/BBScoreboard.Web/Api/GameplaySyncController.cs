using BBScoreboard.Application.Interfaces;
using BBScoreboard.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BBScoreboard.Web.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GameplaySyncController(IGameplayService gameplay) : ControllerBase
{
    [HttpGet("HelloWorld")]
    [AllowAnonymous]
    public IActionResult HelloWorld() => Ok("Hello World");

    [HttpPost("SendAction")]
    public async Task<IActionResult> SendAction(
        [FromQuery] int gameId, [FromQuery] int teamId, [FromQuery] int playerId,
        [FromQuery] int action, [FromQuery] int arg, [FromQuery] int recPlayerId)
    {
        var result = await gameplay.SendActionAsync(gameId, teamId, playerId, action, arg, recPlayerId);
        return Content(result, "application/json");
    }

    [HttpPost("UpdateAction")]
    public async Task<IActionResult> UpdateAction(
        [FromQuery] int id, [FromQuery] int mm, [FromQuery] int ss)
    {
        var result = await gameplay.UpdateActionAsync(id, mm, ss);
        return Ok(result);
    }

    [HttpPost("UpdateTimer")]
    public async Task<IActionResult> UpdateTimer(
        [FromQuery] int gameId, [FromQuery] int start, [FromQuery] DateTime timeLeft,
        [FromQuery] int tlMs, [FromQuery] DateTime timerLastModified, [FromQuery] int tlmMs)
    {
        var result = await gameplay.UpdateTimerAsync(gameId, start, timeLeft, tlMs, timerLastModified, tlmMs);
        return Ok(result);
    }

    [HttpPost("UpdateGame")]
    public async Task<IActionResult> UpdateGame([FromBody] UpdateGameRequest req)
    {
        var result = await gameplay.UpdateGameAsync(
            req.GameId, req.Quarter, req.UpdateScores, req.Ts0, req.Ts1,
            req.UpdateTime, req.TimeLeft, req.TlMs, req.TimerLastModified, req.TlmMs);
        return Ok(result);
    }

    [HttpGet("IsGameStarted")]
    public async Task<IActionResult> IsGameStarted([FromQuery] int gameId)
    {
        var result = await gameplay.IsGameStartedAsync(gameId);
        return Ok(result);
    }

    [HttpGet("GetDelta")]
    public async Task<IActionResult> GetDelta([FromQuery] int gameId, [FromQuery] DateTime lastUpdate)
    {
        var result = await gameplay.GetDeltaAsync(gameId, lastUpdate);
        return Content(result, "application/json");
    }

    [HttpGet("GetDelta2")]
    public async Task<IActionResult> GetDelta2(
        [FromQuery] int gameId, [FromQuery] DateTime lastUpdate, [FromQuery] bool firstSync)
    {
        var result = await gameplay.GetDelta2Async(gameId, lastUpdate, firstSync);
        return Content(result, "application/json");
    }
}

public class UpdateGameRequest
{
    public int GameId { get; set; }
    public int Quarter { get; set; }
    public bool UpdateScores { get; set; }
    public UCGameTeamStat Ts0 { get; set; } = new();
    public UCGameTeamStat Ts1 { get; set; } = new();
    public bool UpdateTime { get; set; }
    public DateTime TimeLeft { get; set; }
    public int TlMs { get; set; }
    public DateTime TimerLastModified { get; set; }
    public int TlmMs { get; set; }
}
