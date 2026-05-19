using cgca.web.client.Services;
using cgca.web.Data;
using cgca.web.Models;
using cgca.web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddBlazorBootstrap();

var dbPath = Path.Combine(builder.Environment.ContentRootPath, "Data", "cgca.db");
Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<ContactSubmissionService>();
builder.Services.AddScoped<SponsorshipSubmissionService>();

builder.Services.AddIdentity<AdminUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/admin/login";
    options.LogoutPath = "/api/auth/logout";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AdminUser>>();
    var adminConfig = app.Configuration.GetSection("AdminSeed");
    var username = adminConfig["Username"] ?? "admin";
    var email = adminConfig["Email"] ?? "admin@cedargrovechristianacademy.org";
    var password = adminConfig["Password"] ?? "Admin@CGCA2026!";

    var existingUser = await userManager.FindByNameAsync(username);
    if (existingUser == null)
    {
        var adminUser = new AdminUser
        {
            UserName = username,
            Email = email,
            EmailConfirmed = true,
            DisplayName = "Administrator"
        };
        await userManager.CreateAsync(adminUser, password);
    }
    else if (string.IsNullOrEmpty(existingUser.DisplayName))
    {
        existingUser.DisplayName = "Administrator";
        await userManager.UpdateAsync(existingUser);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.MapStaticAssets();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorComponents<cgca.web.App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(ChatService).Assembly);

cgca.web.Endpoints.AuthEndpoints.Map(app);
cgca.web.Endpoints.ExportEndpoint.Map(app);

app.Run();
