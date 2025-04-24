# 3.LoginAndExternalApi

## Versionshistorik
- 1.0.0: Oprettet af ECR 10-03-2025
&nbsp;

## Use case
Bygger videre på **2.LoginAndInternalApi**. Der er tilføjet et externt WeatherApi, der implemeterer interfacet `IWeatherForecaster`. 
Når Blazor i første omgang benytter Server-projektet og `GetWeatherForecastAsync()` kaldes, så er det den udgave af metoden som bor i `ServerWeatherforecaster`, 
som køres og som henter data fra WeatherApi.
Når Blazor skifter til WASM (Client-projektet), benyttes `GetWeatherForecastAsync()` i `ClientWeatherForecaster`, som via HttpClient kalder en reverse proxy i Server-projektet.
Proxyen tilføjer AccessToken og sender forespørgslen videre til det eksterne WeatherApi.
Det vil sige at både Server og Client-projektet henter data fra samme eksterne Api, men gør det ad to forskellige veje..

Eksemplet er en opdateret udgave af [Blazor Web App with OpenID Connect (OIDC) (BFF Pattern)](https://github.com/dotnet/blazor-samples/tree/main/9.0/BlazorWebAppOidcBff), 
hvor OIDC er skiftet ud med Auth0.

&nbsp;

## BlazorWebAppAuto

Nugets: 
- Microsoft.Extensions.ServiceDiscovery.Yarp
- Auth0.AspNetCore.Authentication

`appsettings.json` eller *User Secret*s opdateres til følgende:
```json
{
  "Auth0": {
    "Domain": "xxxx.xx.auth0.com",
    "ClientId": "???",
    "ClientSecret": "???",
    "Audience": "https://blazorwebappexternalapi"
  },
  "ExternalApiBaseAdress": "https://localhost:????"
}
```

Til **Program.cs** tilføjes ClientSecret og Audience:

```csharp
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
```

Desuden registreres følgende services:
```csharp
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddHttpForwarder();    // Instantiates YARP's HttpForwarder

builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient<IWeatherForecaster, ServerWeatherForecaster>(httpClient =>
{
    httpClient.BaseAddress = new(builder.Configuration["ExternalApiBaseAdress"]!);
});
```


Og til HTTP request pipeline tilføjes:
```csharp
app.MapForwarder("/weatherforecast", builder.Configuration["ExternalApiBaseAdress"]!, transformBuilder =>
{
    transformBuilder.AddRequestTransform(async transformContext =>
    {
        var accessToken = await transformContext.HttpContext.GetTokenAsync("access_token");
        transformContext.ProxyRequest.Headers.Authorization = new("Bearer", accessToken);
    });
}).RequireAuthorization();
```
&nbsp;

`ServerWeatherForecaster.cs` skal nu have følgende indhold:
```csharp
internal sealed class ServerWeatherForecaster(HttpClient httpClient, IHttpContextAccessor httpContextAccessor) : IWeatherForecaster
{
    public async Task<IEnumerable<WeatherForecast>> GetWeatherForecastAsync()
    {
        var httpContext = httpContextAccessor.HttpContext ??
            throw new InvalidOperationException("No HttpContext available from the IHttpContextAccessor!");

        var accessToken = await httpContext.GetTokenAsync("access_token") ??
            throw new InvalidOperationException("No access_token was saved");

        using var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/weatherforecast");

        requestMessage.Headers.Authorization = new("Bearer", accessToken);

        using var response = await httpClient.SendAsync(requestMessage);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<WeatherForecast[]>() ??
            throw new IOException("No weather forecast!");
    }
}
```




&nbsp;



## BlazorWebAppAuto.Client

Ingen ændringer

&nbsp;


## Weather API
Der oprettes en nyt Minimal ASP.NET WebApi kaldet `WeatherApi`.

Nugets:
- Microsoft.AspNetCore.Authentication.JwtBearer

Program.cs skal have følgende indhold:
```csharp
var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// 👇 new code
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();
// 👆 new code

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection(); 

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.RequireAuthorization()
.WithOpenApi();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
```

&nbsp;

Følgende konfiguration tilføjes enten til appsettings.cs eller User Secrets:
```json
{
  "Authentication": {
    "Schemes": {
      "Bearer": {
        "Authority": "https://xxxx.xx.auth0.com",
        "ValidAudiences": [ ???" ],
        "ValidIssuer": "xxxx.xx.auth0.com"
      }
    }
  }
}
```
