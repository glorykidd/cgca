using System.Net;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace cgca.web.Tests.Integration;

/// <summary>
/// Exercises the real /api/auth/login and /api/auth/logout minimal-API endpoints
/// (antiforgery validation, account lockout) end-to-end over HTTP, since bUnit's
/// component-render tests can't reach these endpoint handlers.
/// </summary>
public class AuthEndpointsTests : IClassFixture<AuthEndpointsTestFactory>
{
    private readonly AuthEndpointsTestFactory _factory;

    public AuthEndpointsTests(AuthEndpointsTestFactory factory)
    {
        _factory = factory;
    }

    // Cookies are threaded through explicitly in each test rather than relying on the
    // client's automatic cookie jar, since a cookie already present on the request can
    // suppress the antiforgery middleware from reissuing a fresh Set-Cookie.
    private HttpClient CreateClient() =>
        _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = false,
        });

    private static async Task<(string token, string cookie)> GetAntiforgeryTokenAsync(
        HttpClient client, string path, string? extraCookie = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        if (extraCookie != null)
            request.Headers.Add("Cookie", extraCookie);

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var setCookies = response.Headers.TryGetValues("Set-Cookie", out var values)
            ? values.ToList()
            : new List<string>();
        var antiforgeryCookie = setCookies.FirstOrDefault(c => c.Contains(".AspNetCore.Antiforgery"))
            ?? throw new InvalidOperationException("No antiforgery cookie returned.");
        var cookie = antiforgeryCookie.Split(';')[0];

        var html = await response.Content.ReadAsStringAsync();
        var match = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        match.Success.Should().BeTrue("the login page should render an antiforgery token");

        return (match.Groups[1].Value, cookie);
    }

    private static FormUrlEncodedContent BuildLoginForm(string token, string username, string password) =>
        new(new Dictionary<string, string>
        {
            ["username"] = username,
            ["password"] = password,
            ["cf-turnstile-response"] = "test-token",
            ["__RequestVerificationToken"] = token,
        });

    [Fact]
    public async Task Login_WithoutAntiforgeryToken_ReturnsBadRequest()
    {
        var client = CreateClient();

        var response = await client.PostAsync("/api/auth/login", BuildLoginForm(
            token: "invalid-or-missing-token",
            username: AuthEndpointsTestFactory.AdminUsername,
            password: AuthEndpointsTestFactory.AdminPassword));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidAntiforgeryTokenAndCredentials_RedirectsToAdmin()
    {
        var client = CreateClient();
        var (token, cookie) = await GetAntiforgeryTokenAsync(client, "/admin/login");

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = BuildLoginForm(token, AuthEndpointsTestFactory.AdminUsername, AuthEndpointsTestFactory.AdminPassword),
        };
        request.Headers.Add("Cookie", cookie);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.OriginalString.Should().Be("/admin");
    }

    [Fact]
    public async Task Login_WithWrongPassword_RedirectsToLoginWithError()
    {
        // Uses its own user rather than the shared seeded admin, so a failed attempt
        // here can never combine with another test's failures to trip account lockout.
        const string username = "wrong-password-test-user";
        var client = CreateClient();
        await SeedAdditionalUserAsync(username, "Correct@Password123!");

        var (token, cookie) = await GetAntiforgeryTokenAsync(client, "/admin/login");

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = BuildLoginForm(token, username, "WrongPassword!1"),
        };
        request.Headers.Add("Cookie", cookie);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.OriginalString.Should().Be("/admin/login?error=1");
    }

    [Fact]
    public async Task Login_AfterFiveFailedAttempts_LocksOutAccountEvenWithCorrectPassword()
    {
        // Uses its own username so this test doesn't fight other tests' lockout state
        // on the shared seeded admin user.
        const string username = "lockout-test-user";
        var client = CreateClient();
        await SeedAdditionalUserAsync(username, "Correct@Password123!");

        for (var attempt = 1; attempt <= 5; attempt++)
        {
            var (token, cookie) = await GetAntiforgeryTokenAsync(client, "/admin/login");
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
            {
                Content = BuildLoginForm(token, username, "WrongPassword!1"),
            };
            request.Headers.Add("Cookie", cookie);

            var response = await client.SendAsync(request);
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        }

        var (finalToken, finalCookie) = await GetAntiforgeryTokenAsync(client, "/admin/login");
        var finalRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = BuildLoginForm(finalToken, username, "Correct@Password123!"),
        };
        finalRequest.Headers.Add("Cookie", finalCookie);

        var finalResponse = await client.SendAsync(finalRequest);

        finalResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);
        finalResponse.Headers.Location!.OriginalString.Should().Be(
            "/admin/login?error=1",
            "the account should be locked out even though the password is correct");
    }

    [Fact]
    public async Task Logout_WithoutAntiforgeryToken_ReturnsBadRequest()
    {
        var client = CreateClient();
        var authCookie = await LogInAsync(client);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>()),
        };
        request.Headers.Add("Cookie", authCookie);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Logout_WithValidAntiforgeryToken_RedirectsHome()
    {
        var client = CreateClient();
        var authCookie = await LogInAsync(client);

        var (token, antiforgeryCookie) = await GetAntiforgeryTokenAsync(client, "/admin/login", authCookie);
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
            }),
        };
        request.Headers.Add("Cookie", $"{authCookie}; {antiforgeryCookie}");

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.OriginalString.Should().Be("/");
    }

    [Fact]
    public async Task Logout_WhenNotAuthenticated_RedirectsToLoginRatherThanSigningOut()
    {
        var client = CreateClient();

        var response = await client.PostAsync("/api/auth/logout", new FormUrlEncodedContent(
            new Dictionary<string, string>()));

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.ToString().Should().Contain("/admin/login");
    }

    private async Task<string> LogInAsync(HttpClient client)
    {
        var (token, cookie) = await GetAntiforgeryTokenAsync(client, "/admin/login");
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = BuildLoginForm(token, AuthEndpointsTestFactory.AdminUsername, AuthEndpointsTestFactory.AdminPassword),
        };
        request.Headers.Add("Cookie", cookie);

        var response = await client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.Redirect, "login should succeed before testing logout");

        var setCookies = response.Headers.TryGetValues("Set-Cookie", out var values)
            ? values.ToList()
            : new List<string>();
        var authCookie = setCookies.FirstOrDefault(c => c.Contains(".AspNetCore.Identity.Application"))
            ?? throw new InvalidOperationException("No auth cookie returned from login.");

        return authCookie.Split(';')[0];
    }

    private async Task SeedAdditionalUserAsync(string username, string password)
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<cgca.web.Models.AdminUser>>();

        var existing = await userManager.FindByNameAsync(username);
        if (existing != null)
            return;

        var user = new cgca.web.Models.AdminUser
        {
            UserName = username,
            Email = $"{username}@example.com",
            EmailConfirmed = true,
            DisplayName = "Lockout Test User",
        };
        var result = await userManager.CreateAsync(user, password);
        result.Succeeded.Should().BeTrue(string.Join(", ", result.Errors.Select(e => e.Description)));
    }
}
