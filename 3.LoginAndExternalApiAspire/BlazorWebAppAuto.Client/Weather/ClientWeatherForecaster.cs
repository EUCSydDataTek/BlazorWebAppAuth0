using System.Net.Http.Json;

namespace BlazorWebAppAuto.Client.Weather;

internal sealed class ClientWeatherForecaster(HttpClient httpClient) : IWeatherForecaster
{
    public async Task<IEnumerable<WeatherForecast>> GetWeatherForecastAsync() =>
        await httpClient.GetFromJsonAsync<WeatherForecast[]>("/weatherforecast") ??
            throw new IOException("No weather forecast!");
}
