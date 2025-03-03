Dette repository indeholder 4 projekter, som viser hvordan en Blazor Web App (.NET-9) tilføjes authentication og authorization med Auth0:

**1.Login** er en demo af en Blazor WebApp med Global Auto-rendering og med Login/Logout. 

**2.LoginAndInternalApi** bygger på 1.Login, men indeholder nu et internt Minimal WebApi (WeatherForecast) i Server-projektet

**3.LoginAndExternalApi** bygger på 2.LoginAndInternalApi, men nu er WeatherForecast Api'et flyttet ud som et eksternt WebApi med authorization.

**3.LoginAndExternalApiAspire** bygger på 2.LoginAndInternalApi, men nu er WeatherForecast Api'et flyttet ud som et eksternt WebApi med authorization. Der benyttes desuden Aspire.

**4.LoginAndExternalApiWithRoles** er en udvidelse af 3.LoginAndExternalApi med "Roles" i WebApi, hvor scopes omsættes til en Policy. Desuden håndteres 404 fejl.

**4.LoginAndExternalApiWithRolesAspire** er en udvidelse af 3.LoginAndExternalApi med "Roles" i WebApi, hvor scopes omsættes til en Policy. Desuden håndteres 404 fejl. Der benyttes desuden Aspire.

