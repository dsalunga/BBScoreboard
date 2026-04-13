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
}
