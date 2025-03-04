using BlazorWebAppAuto.Client.Weather;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Globalization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

builder.Services.AddHttpClient<IWeatherForecaster, ClientWeatherForecaster>(httpClient =>
{
    httpClient.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
});

// Set the client-side culture to danish. Remember <BlazorWebAssemblyLoadAllGlobalizationData> in Program.cs
var culture = new CultureInfo("da-DK");
culture.DateTimeFormat.ShortDatePattern = "dd-MM-yyyy"; // Tving specifikt format (valgfrit)
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

await builder.Build().RunAsync();
