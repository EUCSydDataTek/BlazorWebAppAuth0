using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace BlazorWebAppAuto;

internal static class LoginLogoutEndpointRouteBuilderExtensions
{
    internal static IEndpointConventionBuilder MapLoginAndLogout(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(string.Empty);

        group.MapGet("/Login", async (HttpContext httpContext, string returnUrl = "/") =>
        {
            var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
                    .WithRedirectUri(returnUrl)
                    .Build();

            await httpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
        });

        group.MapPost("/Logout", async (HttpContext httpContext, string returnUrl = "/") =>
        {
            var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
                    .WithRedirectUri(returnUrl)
                    .Build();

            await httpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        });

        return group;
    }
}