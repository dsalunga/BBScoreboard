using System.Net;

namespace BBScoreboard.Tests;

public class AuthRbacTests
{
    private readonly TestWebAppFactory _factory = new();

    [Fact]
    public async Task ApiEndpoint_RequiresAuthentication()
    {
        using var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/api/GameplaySync/IsGameStarted?gameId=1");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ManageUsers_RejectsAuthenticatedNonAdmin()
    {
        using var client = _factory.CreateAuthorizedClient("Scorer");
        var response = await client.GetAsync("/Manage/Referees");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ManageReferees_AllowsAdminRole()
    {
        using var client = _factory.CreateAuthorizedClient("Admin");
        var response = await client.GetAsync("/Manage/Referees");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GameplayApi_IsAccessibleToAuthenticatedUser()
    {
        using var client = _factory.CreateAuthorizedClient("Scorer");
        var response = await client.GetAsync("/api/GameplaySync/IsGameStarted?gameId=7");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GameplayManager_RejectsAuthenticatedReadOnlyUser()
    {
        using var client = _factory.CreateAuthorizedClient();
        var response = await client.GetAsync("/Gameplay/Manager?id=1");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GameplayManager_AllowsScorerRole()
    {
        using var client = _factory.CreateAuthorizedClient("Scorer");
        var response = await client.GetAsync("/Gameplay/Manager?id=1");

        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GameplayMutationApi_RejectsAuthenticatedReadOnlyUser()
    {
        using var client = _factory.CreateAuthorizedClient();
        var response = await client.PostAsync("/api/GameplaySync/SendAction?gameId=1&teamId=2&playerId=3&action=2&arg=2&recPlayerId=-1", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
