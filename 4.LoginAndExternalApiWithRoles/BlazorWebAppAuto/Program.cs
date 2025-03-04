using Auth0.AspNetCore.Authentication;
using BlazorWebAppAuto;
using BlazorWebAppAuto.Client.Weather;
using BlazorWebAppAuto.Components;
using BlazorWebAppAuto.Weather;
using Microsoft.AspNetCore.Authentication;
using Yarp.ReverseProxy.Transforms;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services
    .AddAuth0WebAppAuthentication(options =>
    {
        options.Domain = builder.Configuration["Auth0:Domain"] ?? string.Empty;
        options.ClientId = builder.Configuration["Auth0:ClientId"] ?? string.Empty;
        options.ClientSecret = builder.Configuration["Auth0:ClientSecret"];
    })
    .WithAccessToken(options =>
    {
        options.Audience = builder.Configuration["Auth0:Audience"];
    });

builder.Services.AddAuthorization();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddHttpForwarder();    // Instantiates YARP's HttpForwarder

builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient<IWeatherForecaster, ServerWeatherForecaster>(httpClient =>
{
    httpClient.BaseAddress = new(builder.Configuration["ExternalApiBaseAdress"]!);
});

builder.Services.AddLocalization();

var app = builder.Build();


//////////////////////////////////////// Configure the HTTP request pipeline. ///////////////////////////////////////
app.UseRequestLocalization("da-DK");    // Set the default server culture to Danish https://learn.microsoft.com/en-us/aspnet/core/blazor/globalization-localization?view=aspnetcore-9.0#statically-set-the-server-side-cultur

app.UseStatusCodePagesWithReExecute("/Error/{0}"); // Handles 404 etc.

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapForwarder("/weatherforecast", builder.Configuration["ExternalApiBaseAdress"]!, transformBuilder =>
{
    transformBuilder.AddRequestTransform(async transformContext =>
    {
        var accessToken = await transformContext.HttpContext.GetTokenAsync("access_token");
        transformContext.ProxyRequest.Headers.Authorization = new("Bearer", accessToken);
    });
}).RequireAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorWebAppAuto.Client._Imports).Assembly);

app.MapGroup("/Account").MapLoginAndLogout();

app.Run();