# 3.LoginAndExternalApiAspire

## Versionshistorik
- 1.0.0: Oprettet af ECR 06-02-2025
&nbsp;

## Use case
Et externt WeatherApi projekt er oprettet. Authorization er tilføjet. 
Samme solution som **3.LoginAndExternalApi**, men projekterne styres nu af **Aspire**.

&nbsp;

## Opstart
**AppHost** skal være startup-projekt. Når Aspire viser oversigt over projekterne, klikkes på endpoint for Blazor: `https://localhost:7255/`
&nbsp;


## WeatherApi

Tilføj et standard ASP.NET Core WebApi template, med følgende tilføjelser: **Enlist in .NET Aspire orchestration**.
Derved oprettes både **AppHost** og **ServiceDefaults** projekterne, som her er flyttet til en Solution-folder kaldet **Aspire**.

Indholdet er det samme som i 3.LoginAndExternalApi.
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
&nbsp;

## BlazorWebAppAuto

Skal have en reference til ServiceDefaults-projektet.
&nbsp;

**Program.cs**

Registrering af services er som før, bortset fra at BaseAddress nu er en logisk adresse ("https://weatherapi").

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

Der benyttes en HTTP Forwarder til at håndtere service discovery og routing (YARP) som i forrige projekt, men
også her er der benyttet en logisk adresse ("https://weatherapi"): 
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




