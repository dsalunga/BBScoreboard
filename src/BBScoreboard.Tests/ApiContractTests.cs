using System.Net;
using System.Net.Http.Json;
using BBScoreboard.Domain.Entities;

namespace BBScoreboard.Tests;

public class ApiContractTests
{
    private readonly TestWebAppFactory _factory = new();

    [Fact]
    public async Task HelloWorld_ReturnsExpectedPayload()
    {
        using var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/GameplaySync/HelloWorld");

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal("Hello World", body);
    }

    [Fact]
    public async Task SendAction_BindsQueryParametersAndReturnsServicePayload()
    {
        using var client = _factory.CreateAuthorizedClient("Scorer");
        var response = await client.PostAsync("/api/GameplaySync/SendAction?gameId=10&teamId=20&playerId=30&action=2&arg=1&recPlayerId=11", null);

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"gameId\":10", body);
        Assert.Contains("\"teamId\":20", body);
        Assert.Contains("\"playerId\":30", body);
        Assert.Contains("\"action\":2", body);
        Assert.Contains("\"arg\":1", body);
        Assert.Contains("\"recPlayerId\":11", body);
    }

    [Fact]
    public async Task UpdateAction_ReturnsOkInt()
    {
        using var client = _factory.CreateAuthorizedClient("Scorer");
        var response = await client.PostAsync("/api/GameplaySync/UpdateAction?id=7&mm=1&ss=2", null);

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal("10", body);
    }

    [Fact]
    public async Task UpdateTimer_ReturnsOkInt()
    {
        using var client = _factory.CreateAuthorizedClient("Scorer");
        var lastModified = WebUtility.UrlEncode(DateTime.UnixEpoch.ToString("O"));
        var timeLeft = WebUtility.UrlEncode(DateTime.UnixEpoch.AddMinutes(10).ToString("O"));
        var response = await client.PostAsync($"/api/GameplaySync/UpdateTimer?gameId=5&start=1&timeLeft={timeLeft}&tlMs=2&timerLastModified={lastModified}&tlmMs=3", null);

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal("11", body);
    }

    [Fact]
    public async Task UpdateGame_BindsBodyAndReturnsOkInt()
    {
        using var client = _factory.CreateAuthorizedClient("Scorer");
        var payload = new
        {
            gameId = 99,
            quarter = 4,
            updateScores = true,
            ts0 = new UCGameTeamStat(),
            ts1 = new UCGameTeamStat(),
            updateTime = false,
            timeLeft = DateTime.UnixEpoch,
            tlMs = 0,
            timerLastModified = DateTime.UnixEpoch,
            tlmMs = 0
        };
        var response = await client.PostAsJsonAsync("/api/GameplaySync/UpdateGame", payload);

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal("104", body);
    }

    [Fact]
    public async Task IsGameStarted_ReturnsOkInt()
    {
        using var client = _factory.CreateAuthorizedClient("Scorer");
        var response = await client.GetAsync("/api/GameplaySync/IsGameStarted?gameId=5");

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal("1", body);
    }

    [Fact]
    public async Task GetDelta_ReturnsJsonPayload()
    {
        using var client = _factory.CreateAuthorizedClient("Scorer");
        var lastUpdate = WebUtility.UrlEncode(DateTime.UnixEpoch.ToString("O"));
        var response = await client.GetAsync($"/api/GameplaySync/GetDelta?gameId=3&lastUpdate={lastUpdate}");

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"gameId\":3", body);
    }

    [Fact]
    public async Task GetDelta2_ReturnsJsonPayload()
    {
        using var client = _factory.CreateAuthorizedClient("Scorer");
        var lastUpdate = WebUtility.UrlEncode(DateTime.UnixEpoch.ToString("O"));
        var response = await client.GetAsync($"/api/GameplaySync/GetDelta2?gameId=4&lastUpdate={lastUpdate}&firstSync=true");

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"gameId\":4", body);
        Assert.Contains("\"firstSync\":true", body);
    }
}
