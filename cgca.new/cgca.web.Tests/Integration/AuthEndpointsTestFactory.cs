using cgca.web.Data;
using cgca.web.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace cgca.web.Tests.Integration;

/// <summary>
/// Boots the real app (real Identity, lockout, and antiforgery pipeline) against an
/// isolated in-memory SQLite database, with Turnstile stubbed out. Program.cs calls
/// Database.Migrate() unconditionally, which the EF Core InMemory provider doesn't
/// support, so a real (but ephemeral) SQLite connection is used instead.
/// </summary>
public class AuthEndpointsTestFactory : WebApplicationFactory<Program>
{
    public const string AdminUsername = "admin";
    public const string AdminPassword = "Test@Password123!";

    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();

        builder.UseSetting("AdminSeed:Username", AdminUsername);
        builder.UseSetting("AdminSeed:Password", AdminPassword);
        builder.UseSetting("Turnstile:SiteKey", "test-site-key");
        builder.UseSetting("Turnstile:SecretKey", "test-secret-key");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));

            services.RemoveAll<TurnstileService>();
            services.AddSingleton<TurnstileService, StubTurnstileService>();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _connection.Dispose();
    }
}
