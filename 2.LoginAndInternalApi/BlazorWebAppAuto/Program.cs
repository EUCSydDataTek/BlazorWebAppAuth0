using Auth0.AspNetCore.Authentication;
using BlazorWebAppAuto;
using BlazorWebAppAuto.Client.Weather;
using BlazorWebAppAuto.Components;
using BlazorWebAppAuto.Weather;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services
    .AddAuth0WebAppAuthentication(options =>
    {
        options.Domain = builder.Configuration["Auth0:Domain"]!;
        options.ClientId = builder.Configuration["Auth0:ClientId"]!;
    });

builder.Services.AddAuthorization();

// 👇 new code
builder.Services.AddScoped<IWeatherForecaster, ServerWeatherForecaster>();
// 👆 new code

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

// 👇 new code
app.MapGet("/weatherforecast", ([FromServices] IWeatherForecaster WeatherForecaster) =>
{
    return WeatherForecaster.GetWeatherForecastAsync();
}).RequireAuthorization();
// 👆 new code

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorWebAppAuto.Client._Imports).Assembly);

app.MapGroup("/Account").MapLoginAndLogout();

app.Run();

