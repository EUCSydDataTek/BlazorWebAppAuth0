var builder = DistributedApplication.CreateBuilder(args);

var weatherApi = builder.AddProject<Projects.WeatherApi>("weatherapi");

builder.AddProject<Projects.BlazorWebAppAuto>("blazorfrontend")
    .WithReference(weatherApi);
    

builder.Build().Run();
