using cgca.web.Models;
using cgca.web.Services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;

namespace cgca.web.Endpoints;

public static class AuthEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/auth/login", async (
            HttpContext context,
            SignInManager<AdminUser> signInManager,
            TurnstileService turnstile,
            IAntiforgery antiforgery) =>
        {
            try
            {
                await antiforgery.ValidateRequestAsync(context);
            }
            catch (AntiforgeryValidationException)
            {
                return Results.BadRequest();
            }

            var form = context.Request.Form;
            var username = form["username"].ToString();
            var password = form["password"].ToString();
            var captchaToken = form["cf-turnstile-response"].ToString();

            var captchaValid = await turnstile.VerifyAsync(captchaToken, context.Connection.RemoteIpAddress?.ToString());
            if (!captchaValid)
                return Results.Redirect("/admin/login?error=1");

            var result = await signInManager.PasswordSignInAsync(
                username, password, isPersistent: false, lockoutOnFailure: true);

            return result.Succeeded
                ? Results.Redirect("/admin")
                : Results.Redirect("/admin/login?error=1");
        });

        app.MapPost("/api/auth/logout", async (
            HttpContext context,
            SignInManager<AdminUser> signInManager,
            IAntiforgery antiforgery) =>
        {
            try
            {
                await antiforgery.ValidateRequestAsync(context);
            }
            catch (AntiforgeryValidationException)
            {
                return Results.BadRequest();
            }

            await signInManager.SignOutAsync();
            return Results.Redirect("/");
        }).RequireAuthorization();
    }
}
