# 2.LoginAndInternalApi

## Versionshistorik
- 1.0.0: Oprettet af ECR 06-02-2025
&nbsp;

## Use case
Bygger videre på **1.Login projektet**. Der er tilføjet et "WeatherAPI" i Server-projektet i form af metoden: `GetWeatherForecastAsync()`, der 
implemeterer interfacet `IWeatherForecaster`. Når Blazor i første omgang benytter Server-projektet, benyttes denne metode.
Når Blazor skifter til Client-projektet, benyttes `ClientWeatherForecaster`, der benytter `HttpClient` til at kalde Server-projektets WeatherAPI.
Det betyder at både Server og Client-projektet henter data fra samme sted.

For at undgå at data hentes flere gange (Pre-rendering samt normal rendering), gemmes data i **ApplicationState**, således at data kun hentes én gang. Dette sker vha. af 
servicen `PersistentComponentState`, som injectes i `WeatherForecast.razor`. I første omgang hentes data fra `ServerWeatherForecaster`, og gemmes i ApplicationState.
Når der skiftes til `ClientWeatherForecaster`, hentes data fra `ApplicationState`, og ikke fra `ServerWeatherForecaster`.

Eksemplet er en opdateret udgave af [Call Protected APIs from a Blazor Web App](https://auth0.com/blog/call-protected-api-from-blazor-web-app/), 
hvor WASM projektet kalder et Internal Api i Server-projektet.

&nbsp;

## BlazorWebAppAuto

Til **Program.cs** tilføjes følgende service:

```csharp
builder.Services.AddScoped<IWeatherForecaster, ServerWeatherForecaster>();
```

Og til HTTP request pipeline tilføjes:
```csharp
app.MapGet("/weatherforecast", ([FromServices] IWeatherForecaster WeatherForecaster) =>
{
    return WeatherForecaster.GetWeatherForecastAsync();
}).RequireAuthorization();
```

&nbsp;

**Weather API**

Der oprettes en ny fil Weather/ServerWeatherForecaster.cs med følgende indhold:
```csharp
using BlazorWebAppAuto.Client.Weather;

namespace BlazorWebAppAuto.Weather;

public class ServerWeatherForecaster() : IWeatherForecaster
{
    public readonly string[] summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    public async Task<IEnumerable<WeatherForecast>> GetWeatherForecastAsync()
    {
        // Simulate asynchronous loading to demonstrate streaming rendering
        await Task.Delay(500);

        return Enumerable.Range(1, 5).Select(index =>
            new WeatherForecast
            (
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                summaries[Random.Shared.Next(summaries.Length)]
            ))
        .ToArray();
    }
}
```


&nbsp;

## BlazorWebAppAuto.Client
#### Nugets
`<PackageReference Include="Microsoft.Extensions.Http" Version="9.0.*" />`

&nbsp;

Til Program.cs tilføjes følgende service:
```csharp
builder.Services.AddHttpClient<IWeatherForecaster, ClientWeatherForecaster>(httpClient =>
{
    httpClient.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
});
```

&nbsp;

#### Weather API

I folderen **Weather** oprettes følgende 3 filer:

`WeatherForecast.cs`:
```csharp
public sealed class WeatherForecast(DateOnly date, int temperatureC, string summary)
{
    public DateOnly Date { get; set; } = date;
    public int TemperatureC { get; set; } = temperatureC;
    public string? Summary { get; set; } = summary;
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
```

&nbsp;


`IWeatherForecaster.cs`
```csharp
public interface IWeatherForecaster
{
    Task<IEnumerable<WeatherForecast>> GetWeatherForecastAsync();
}
```

&nbsp;

`ClientWeatherForecaster.cs`
```csharp
internal sealed class ClientWeatherForecaster(HttpClient httpClient) : IWeatherForecaster
{
    public async Task<IEnumerable<WeatherForecast>> GetWeatherForecastAsync() =>
        await httpClient.GetFromJsonAsync<WeatherForecast[]>("/weatherforecast") ??
            throw new IOException("No weather forecast!");
}
```

&nbsp;

#### Weather.razor

Følgende tilføjes øverst i filen:
```csharp
@using BlazorWebAppAuto.Client.Weather
@implements IDisposable
@inject PersistentComponentState ApplicationState
@inject IWeatherForecaster WeatherForecaster
```

Code-delen ændres til:
```csharp
@code {
    private IEnumerable<WeatherForecast>? forecasts;
    private PersistingComponentStateSubscription persistingSubscription;

    protected override async Task OnInitializedAsync()
    {
        persistingSubscription = ApplicationState.RegisterOnPersisting(PersistData);

        if (!ApplicationState.TryTakeFromJson<IEnumerable<WeatherForecast>>(nameof(forecasts), out var restoredData))
        {
            forecasts = await WeatherForecaster.GetWeatherForecastAsync();
        }
        else
        {
            forecasts = restoredData!;
        }
    }

    private Task PersistData()
    {
        ApplicationState.PersistAsJson(nameof(forecasts), forecasts);

        return Task.CompletedTask;
    }

    void IDisposable.Dispose() => persistingSubscription.Dispose();
}
```
