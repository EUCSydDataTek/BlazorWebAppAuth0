# 4.LoginAndExternalApiWithRoles

## Versionshistorik
- 1.0.1: Oprettet af ECR 02-03-2025
&nbsp;

## Use case
I Auth0 er der oprettet to *Permissions*, samt to *Roles*, der knytter Permissions sammen. 

Der er lavet håndtering af en 404 i tilfælde af at der webbes et ugyldigt endpoint.
&nbsp;

## Opstart
Højre klik på Solution og sæt både WeatherApi og BlazorWebAppAuto til at starte.
&nbsp;

## Auth0
Der oprettes nogle *Permissions*, som samles i *Roles*. Gå ind under *Applications | APIs*, vælg det aktuelle Api og klik på fanen *Permissions*.
Her kan der oprettes Permissions, som kan tildeles til Roller. Opret f.eks. en Permission med navnet *read:weatherforecast* og en Permission med navnet *write:weatherforecast*.

Under *User Management | Roles* kan der oprettes *Roles*, der tildeles *Permissions* og som til sidst tilknyttes en *User*. 
Opret f.eks. en Role med navnet: **ReadWeatherRole** og tildel den Permissionen *read:weatherforecast*
ved at gå til Permissions-fanen. 
På samme måde oprettes en Role med navnet: **ReadWriteWeatherRole** og den tildeles både *read:weatherforecast* og *write:weatherforecast* Permissions.

I eksemplet er der også oprettet en Role med navnet **Administrator**. 

Tilsidst går man til *User Management | Users* og tildeler den ønskede *User* de(n) nye roller.

&nbsp;

## BlazorWebAppAuto.Server
Tilføj følgende i `Program.cs`, som den første linje i HTTP-pipelinen:
```csharp
app.UseStatusCodePagesWithReExecute("/Error/{0}");
```
Se evt. [404 Handling](https://stackoverflow.com/questions/78102853/how-do-i-provide-the-missing-404-handling-for-visual-studios-blazor-web-app)

&nbsp;

## BlazorWebAppAuto.Client

I `Pages/UserClaims.razor` ændres `[Authorize]` attributten til:
```html
@attribute [Authorize(Roles = "Administrator")]
```
&nbsp;

### 404 håndtering

Der oprettes en ny component i Pages, kaldet `NotFound.razor`. Denne vises, hvis der navigeres til et ugyldigt endpoint.
```html
@page "/Error/404"
@page "/NotFound"

<h3>Page Not Found</h3>
<p>Sorry, the page you are looking for does not exist.</p>
```

I componenten *Routes.razor* udvides switch'en med en Route til `NotFound`:
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
&nbsp;
### Menu

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

Her erstattes `AddAuthorization()` med følgende:
```csharp
builder.Services.AddAuthorizationBuilder()
  .AddPolicy("ReadPolicy", p => p.RequireAuthenticatedUser().RequireClaim("permissions", "read:weatherforecast"))
  .AddPolicy("WritePolicy", p => p.RequireAuthenticatedUser().RequireClaim("permissions", "write:weatherforecast"));
```

&nbsp;


Desuden tilføjes `.RequireAuthorization("ReadPolicy")` efter `app.MapGet()`.

&nbsp;

