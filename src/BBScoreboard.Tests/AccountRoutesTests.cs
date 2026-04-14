using System.Net;

namespace BBScoreboard.Tests;

public class AccountRoutesTests
{
    private readonly TestWebAppFactory _factory = new();

    [Theory]
    [InlineData("/Account/ForgotPassword")]
    [InlineData("/Account/PasswordReset")]
    [InlineData("/Account/Confirm")]
    [InlineData("/Account/RegisterService")]
    [InlineData("/Account/ExternalLoginFailure")]
    [InlineData("/Account/Thanks")]
    [InlineData("/Account/AccountLockedOut")]
    public async Task AnonymousAccountPages_AreReachable(string path)
    {
        using var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AccountLogin_RedirectsToMainLogin()
    {
        using var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/Account/Login");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Login", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task ManageAccount_RequiresAuthentication()
    {
        using var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/Account/Manage");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ManageAccount_AllowsAuthenticatedUser()
    {
        using var client = _factory.CreateAuthorizedClient();
        var response = await client.GetAsync("/Account/Manage");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
