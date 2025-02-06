# 3.LoginAndExternalApi

## Versionshistorik
- 1.0.0: Oprettet af ECR 06-02-2025
&nbsp;

## Use case
Et externt WeatherApi projekt er oprettet. Authorization er tilføjet. Projekterne styres nu af Aspire.

Eksemplet er delvist bygget over [Call Protected APIs from a Blazor Web App](https://auth0.com/blog/call-protected-api-from-blazor-web-app/), 
hvor WASM projektet også kalder et External Api, men gør det via en Proxy-service i Server-projektet (BFF-pattern).

&nbsp;

## Auth0
Der oprettes et Api i Auth0, som f.eks. hedder *Blazor Web App External API*.

Der skrives en Identifier, som f.eks. kan være: `https://blazorwebappexternalapi` (blot en logisk adresse).
Denne Identifier skal bruges i projektet, når der skal tilgås API'et, omtales der som `Audience`.

&nbsp;


## WeatherApi

Tilføj et standard ASP.NET Core WebApi template, med følgende tilføjelser: **Enlist in .NET Aspire orchestration**

Nuget:
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.*" />
```
&nbsp;

#### appsettings.json
Tilføj følgende:
```json
{
  "Authentication": {
    "Schemes": {
      "Bearer": {
        "Authority": "https://xxxxxx.eu.auth0.com",
        "ValidAudiences": [ "https://xxxxxxxxxxxxxxxx" ],
        "ValidIssuer": "xxxxxx.eu.auth0.com"
      }
    }
  }
}
```


#### Program.cs

Her tilføjes følgende:
```csharp
// 👇 new code
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();
// 👆 new code
```

Desuden tilføjes `.RequireAuthorization()` efter app.MapGet()

&nbsp;

## AppHost

AppHost-projektet skal også have en reference til BlazorWebAppAuto-projektet.

Program.cs ser således ud:
```csharp
var builder = DistributedApplication.CreateBuilder(args);

var weatherApi = builder.AddProject<Projects.WeatherApi>("weatherapi");

builder.AddProject<Projects.BlazorWebAppAuto>("blazorfrontend")
    .WithReference(weatherApi);
    

builder.Build().Run();
```







## BlazorWebAppAuto

Skal have en reference til ServiceDefaults-projektet.

**Nuget**

```xml
<PackageReference Include="Microsoft.Extensions.ServiceDiscovery.Yarp" Version="9.0.*" />
```

&nbsp;

**Program.cs**

Både ClientSecret og Audience skal benyttes til at kalde det eksterne API. 
```csharp
// Add service defaults & Aspire components.
builder.AddServiceDefaults();

builder.Services
    .AddAuth0WebAppAuthentication(options =>
    {
        options.Domain = builder.Configuration["Auth0:Domain"] ?? string.Empty;
        options.ClientId = builder.Configuration["Auth0:ClientId"] ?? string.Empty;
        // 👇 new code
        options.ClientSecret = builder.Configuration["Auth0:ClientSecret"];
        // 👆 new code
    })
    // 👇 new code
    .WithAccessToken(options =>
    {
        options.Audience = builder.Configuration["Auth0:Audience"];
    });
// 👆 new code
```

Der tilføjes forskellige services:

```csharp
// 👇 new code
builder.Services.AddHttpForwarderWithServiceDiscovery();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<IWeatherForecaster, ServerWeatherForecaster>(httpClient =>
{
    httpClient.BaseAddress = new("https://weatherapi");
});
// 👆 new code
```` 

Der benyttes en HTTP Forwarder til at håndtere service discovery og routing (YARP): 
```csharp
// 👇 new code
app.MapForwarder("/weatherforecast", "https://weatherapi", transformBuilder =>
{
    transformBuilder.AddRequestTransform(async transformContext =>
    {
        var accessToken = await transformContext.HttpContext.GetTokenAsync("access_token");
        transformContext.ProxyRequest.Headers.Authorization = new("Bearer", accessToken);
    });
}).RequireAuthorization();
// 👆 new code
```

#### Appsettings.json 
```json
{
  "Auth0": {
    "Domain": "your-auth0-domain",
    "ClientId": "your-auth0-client-id",
    "ClientSecret": "your-auth0-client-secret", // 👈 new key
    "Audience": "your-auth0-audience"           // 👈 new key
  }
}
```

#### ServerWeatherForecaster
Denne klasse er nu ændret til at benytte en HttpClient og en service discovery:
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

## Opstart
**AppHost** skal være startup-projekt. Når Aspire viser oversigt over projekterne, klikkes på endpoint for Blazor: https://localhost:7255/
&nbsp;

