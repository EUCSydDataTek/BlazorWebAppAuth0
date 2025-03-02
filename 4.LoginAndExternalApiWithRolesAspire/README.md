# 4.LoginAndExternalApiWithRoles

## Versionshistorik
- 1.0.0: Oprettet af ECR 06-02-2025
&nbsp;

## Use case
I Auth0 er der oprettet to Permissions, samt to roller der knytter Permissions sammen. 
Der er lavet håndtering af en 404 ifald der webbes et ugyldigt endpoint.
Projekterne styres af Aspire.
&nbsp;

## Opstart
**AppHost** skal være startup-projekt. Når Aspire viser oversigt over projekterne, klikkes på endpoint for Blazor: https://localhost:7255/
&nbsp;

## Auth0
Der oprettes nogle Permissions, som samles i Roller. Gå ind under *Applications/APIs*, vælg det aktuelle Api og klik på fanen *Permissions*.
Her kan der oprettes Permissions, som kan tildeles til Roller. Opret f.eks. en Permission med navnet *read:weatherforecast* og en Permission med navnet *write:weatherforecast*.

Under *User Management/Roles* kan der oprettes Roller, som kan tildeles til brugere. Opret f.eks. en Rolle med navnet *WeatherReader* og en Rolle med navnet *WeatherWriter*. 
Disse Roller tildeles de respektive Permissions. Opret f.eks. en rolle med navnet: **ReadWeatherRole** og tildel den Permissionen *read:weatherforecast*
ved at gå til Permissions-fanen. 
På samme måde kan der oprettes en rolle med navnet: **ReadWriteWeatherRole** og tildel den Permissionen både *read:weatherforecast* og *write:weatherforecast*.

I eksemplet er der også oprettet en Rolle med navnet **Administrator**. 

Tilsidst går man til Users-fanen og tildeler brugeren de nye roller.

&nbsp;

## BlazorWebAppAuto.Server
Tilføj følgende i Program.cs, som den første linje i HTTP-pipelinen:
```csharp
app.UseStatusCodePagesWithReExecute("/Error/{0}");
```
Se evt. [404 Handling](https://stackoverflow.com/questions/78102853/how-do-i-provide-the-missing-404-handling-for-visual-studios-blazor-web-app)

&nbsp;

## BlazorWebAppAuto.Client

I Pages/UserClaims.razor ændres Authorize attributten til:
```html
@attribute [Authorize(Roles = "Administrator")]
```


**404 håndtering**

Der oprettes en ny component i Pages, kaldet NotFound.razor. Denne vises, hvis der navigeres til et ugyldigt endpoint.
```html
@page "/Error/404"
@page "/NotFound"

<h3>Page Not Found</h3>
<p>Sorry, the page you are looking for does not exist.</p>
```

I componenten *Routes.razor* udvides switch'en med en Route til NotFound:
```html
@using BlazorWebAppAuto.Client.Pages

<Router AppAssembly="typeof(Program).Assembly">
    <Found Context="routeData">
        <AuthorizeRouteView RouteData="routeData" DefaultLayout="typeof(Layout.MainLayout)" >
            <NotAuthorized>
                <RedirectToLogin />
            </NotAuthorized>
        </AuthorizeRouteView>
        <FocusOnNavigate RouteData="routeData" Selector="h1" />
    </Found>
   <NotFound>
        <LayoutView Layout="@typeof(Layout.MainLayout)">
            <NotFound />
        </LayoutView>
     </NotFound>
</Router>
```

I NavMenu componenten vises om brugeren er medlem af Administrator-rollen. 
I menuen er der lavet følgende logik:
- Hvis brugeren er logget ind, men ikke medlem af Administrator-rollen, 
vises "Counter".
- Hvis brugeren er logget ind og medlem af Administrator-rollen, vises også "user-claims".
```html
<div class="top-row text-center text-white">
    <AuthorizeView>
        <Authorized Context="authContext">
            <AuthorizeView Roles="Administrator" Context="adminContext">
                <Authorized>
                    Hi admin!
                </Authorized>
                <NotAuthorized>
                    You are not an admin =(
                </NotAuthorized>
            </AuthorizeView>
        </Authorized>
        <NotAuthorized>
            You are not authenticated =(
        </NotAuthorized>
    </AuthorizeView>
</div>


<div class="top-row ps-3 navbar navbar-dark">
    <div class="container-fluid">
        <a class="navbar-brand" href="">BlazorIntAuto</a>
    </div>
</div>

<input type="checkbox" title="Navigation menu" class="navbar-toggler" />

<div class="nav-scrollable" onclick="document.querySelector('.navbar-toggler').click()">
    <nav class="nav flex-column">
        <div class="nav-item px-3">
            <NavLink class="nav-link" href="" Match="NavLinkMatch.All">
                <span class="bi bi-house-door-fill-nav-menu" aria-hidden="true"></span> Home
            </NavLink>
        </div>

        <div class="nav-item px-3">
            <NavLink class="nav-link" href="counter">
                <span class="bi bi-plus-square-fill-nav-menu" aria-hidden="true"></span> Counter
            </NavLink>
        </div>

        <LogInOrOut />

        <AuthorizeView>
            <Authorized Context="authContext">
                <div class="nav-item px-3">
                    <NavLink class="nav-link" href="weather">
                        <span class="bi bi-list-nested-nav-menu" aria-hidden="true"></span> Weather
                    </NavLink>
                </div>
                <AuthorizeView Roles="Administrator" Context="adminContext">
                    <Authorized>
                        <div class="nav-item px-3">
                            <NavLink class="nav-link" href="user-claims">
                                <span class="bi bi-list-nested-nav-menu" aria-hidden="true"></span> User Claims
                            </NavLink>
                        </div>
                    </Authorized>
                </AuthorizeView>
            </Authorized>
        </AuthorizeView>
    </nav>
</div>
```




&nbsp;

## WeatherApi

#### Program.cs

Her erstattes AddAuthorization() med følgende:
```csharp
builder.Services.AddAuthorizationBuilder()
  .AddPolicy("ReadPolicy", p => p.RequireAuthenticatedUser().RequireClaim("permissions", "read:weatherforecast"))
  .AddPolicy("WritePolicy", p => p.RequireAuthenticatedUser().RequireClaim("permissions", "write:weatherforecast"));
```

&nbsp;


Desuden tilføjes `.RequireAuthorization("ReadPolicy")` efter app.MapGet().

&nbsp;

