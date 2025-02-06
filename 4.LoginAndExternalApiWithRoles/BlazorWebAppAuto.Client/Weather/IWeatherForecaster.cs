namespace BlazorWebAppAuto.Client.Weather;

public interface IWeatherForecaster
{
    Task<IEnumerable<WeatherForecast>> GetWeatherForecastAsync();
}
