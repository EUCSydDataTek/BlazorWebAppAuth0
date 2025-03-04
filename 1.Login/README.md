# 1. Login

## Versionshistorik
- 1.0.0: Oprettet af ECR 06-02-2025
&nbsp;

## UseCase
Demonstrerer Authentication med Auth0 i et .NET 9 Blazor WebApp projekt med Auto Rendering.

- Home: Anonymous access
- Counter: Authenticated access, men er synlig for ikke-authentikerede brugere
- Logout: Authenticated access
- Weather: Authenticated access
- UserClaims: Authenticated access

Menuen viser tilgængelige sider afhængig af om brugeren er logget ind eller ej.

Eksemplet er en opdateret udgave af [Add Auth0 Authentication to Blazor Web Apps](https://auth0.com/blog/auth0-authentication-blazor-web-apps/) .

&nbsp;

## Auth0
- Opret en *Application* som en *Regular Web Application*, her kaldet `Blazor Web App`
- Udfyld *Allowed Callback URLs* med aktuel adresse, f.eks. `https://localhost:7255/callback`
- Udfyld *Allowed Logout URLs* med aktuel adresse, f.eks. `https://localhost:7255/`
- Resten er default værdier

`appsettings.json` i *BlazorWebAppAuto* projektet skal udfyldes med de nødvendige værdier fra Auth0:
```json
{
  "Auth0": {
	"Domain": "dev-xxxxxx.eu.auth0.com",
	"ClientId": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
}
```
Alternativt kan man benytte *User Secrets* til at gemme disse værdier (anbefales).)

&nbsp;

## BlazorWebAppAuto

**Nugets**

`<PackageReference Include="Auth0.AspNetCore.Authentication" Version="1.4.*" />`
&nbsp;

**Program.cs**

Tilføj følgende til **Program.cs**:
```csharp
builder.Services
    .AddAuth0WebAppAuthentication(options =>
    {
        options.Domain = builder.Configuration["Auth0:Domain"]!;
        options.ClientId = builder.Configuration["Auth0:ClientId"]!;
    });

builder.Services.AddAuthorization();
```

Tilføj `.AddAuthenticationStateSerialization()` til `builder.Services.AddRazorComponents()`

Tilføj `app.MapGroup("/Account").MapLoginAndLogout();` tilsidst for at tilføje
Login og Logout endpoints fra klassen `LoginLogoutEndpointRouteBuilderExtensions`.
Denne nye klasse oprettes i `BlazorWebAppAuto.Server` projektet.

&nbsp;

**LoginLogoutEndpointRouteBuilderExtensions**

Opret en ny klasse i `BlazorWebAppAuto.Server` projektet kaldet `LoginLogoutEndpointRouteBuilderExtensions.cs`:
```csharp
using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

internal static class LoginLogoutEndpointRouteBuilderExtensions
{
    internal static IEndpointConventionBuilder MapLoginAndLogout(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(string.Empty);

        group.MapGet("/Login", async (HttpContext httpContext, string returnUrl = "/") =>
        {
            var authenticationProperties = new LoginAuthenticationPropertiesBuilder()
                    .WithRedirectUri(returnUrl)
                    .Build();

            await httpContext.ChallengeAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
        });

        group.MapPost("/Logout", async (HttpContext httpContext, string returnUrl = "/") =>
        {
            var authenticationProperties = new LogoutAuthenticationPropertiesBuilder()
                    .WithRedirectUri(returnUrl)
                    .Build();

            await httpContext.SignOutAsync(Auth0Constants.AuthenticationScheme, authenticationProperties);
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        });

        return group;
    }
}
```

&nbsp;

## BlazorWebAppAuto.Client

**Nugets**

`<PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly.Authentication" Version="9.0.*" />`
&nbsp;

I _Imports.razor tilføjes:
```csharp
@using Microsoft.AspNetCore.Authorization
@using Microsoft.AspNetCore.Components.Authorization
```

Program.cs

Tilføj følgende til Program.cs:
```csharp
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();
```

&nbsp;

Opret filen **RedirectToLogin.razor** i roden af projektet:

```csharp
@inject NavigationManager Navigation

@code {
    protected override void OnInitialized()
    {
        Navigation.NavigateTo($"Account/Login?returnUrl={Uri.EscapeDataString(Navigation.Uri)}", forceLoad: true);
    }
}
```

Opret også den lokale css-fil: LogInOrOut.razor.css med dette indhold:
```css
.bi {
    display: inline-block;
    position: relative;
    width: 1.25rem;
    height: 1.25rem;
    margin-right: 0.75rem;
    top: -1px;
    background-size: cover;
}

.bi-person-badge-nav-menu {
    background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='16' height='16' fill='white' class='bi bi-person-badge' viewBox='0 0 16 16'%3E%3Cpath d='M6.5 2a.5.5 0 0 0 0 1h3a.5.5 0 0 0 0-1h-3zM11 8a3 3 0 1 1-6 0 3 3 0 0 1 6 0z'/%3E%3Cpath d='M4.5 0A2.5 2.5 0 0 0 2 2.5V14a2 2 0 0 0 2 2h8a2 2 0 0 0 2-2V2.5A2.5 2.5 0 0 0 11.5 0h-7zM3 2.5A1.5 1.5 0 0 1 4.5 1h7A1.5 1.5 0 0 1 13 2.5v10.795a4.2 4.2 0 0 0-.776-.492C11.392 12.387 10.063 12 8 12s-3.392.387-4.224.803a4.2 4.2 0 0 0-.776.492V2.5z'/%3E%3C/svg%3E");
}

.bi-arrow-bar-left-nav-menu {
    background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='16' height='16' fill='white' class='bi bi-arrow-bar-left' viewBox='0 0 16 16'%3E%3Cpath d='M12.5 15a.5.5 0 0 1-.5-.5v-13a.5.5 0 0 1 1 0v13a.5.5 0 0 1-.5.5ZM10 8a.5.5 0 0 1-.5.5H3.707l2.147 2.146a.5.5 0 0 1-.708.708l-3-3a.5.5 0 0 1 0-.708l3-3a.5.5 0 1 1 .708.708L3.707 7.5H9.5a.5.5 0 0 1 .5.5Z'/%3E%3C/svg%3E");
}

.nav-item {
    font-size: 0.9rem;
    padding-bottom: 0.5rem;
    text-align: left;
}

    .nav-item .nav-link {
        color: #d7d7d7;
        background: none;
        border: none;
        border-radius: 4px;
        height: 3rem;
        display: flex;
        align-items: center;
        text-align: left;
        width: 100%;
    }

.nav-item .nav-link:hover {
    background-color: rgba(255,255,255,0.1);
    color: white;
}
```

&nbsp;

**Routes.razor** ændres til:
```xml
<Router AppAssembly="typeof(Program).Assembly">
    <Found Context="routeData">
        <AuthorizeRouteView RouteData="routeData" DefaultLayout="typeof(Layout.MainLayout)" >
            <NotAuthorized>
                <RedirectToLogin />
            </NotAuthorized>
        </AuthorizeRouteView>
        <FocusOnNavigate RouteData="routeData" Selector="h1" />
    </Found>
</Router>
```

&nbsp;

Under Pages oprettes en ny component kaldet UserClaims.razor:
```csharp
@page "/user-claims"
@using System.Security.Claims
@using Microsoft.AspNetCore.Authorization
@attribute [Authorize]

<PageTitle>User Claims</PageTitle>

<h1>User Claims</h1>

@if (claims.Any())
{
    <ul>
        @foreach (var claim in claims)
        {
            <li><b>@claim.Type:</b> @claim.Value</li>
        }
    </ul>
}

@code {
    private IEnumerable<Claim> claims = [];

    [CascadingParameter]
    private Task<AuthenticationState>? AuthState { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (AuthState == null)
        {
            return;
        }

        var authState = await AuthState;
        claims = authState.User.Claims;
    }
}
```
&nbsp;

Følgende Pages får tilføjelsen: `@attribute [Authorize]`

- Counter.razor
- Weather.razor
- UserClaims.razor

&nbsp;

Opret filen **LogInOrOut.razor** i Layout mappen:
```csharp
@implements IDisposable
@inject NavigationManager Navigation

<div class="nav-item px-3">
    <AuthorizeView>
        <Authorized>
            <form action="Account/Logout" method="post">
                <AntiforgeryToken />
                <input type="hidden" name="ReturnUrl" value="@currentUrl" />
                <button type="submit" class="nav-link">
                    <span class="bi bi-arrow-bar-left-nav-menu" aria-hidden="true"></span> Logout @context.User.Identity?.Name
                </button>
            </form>
        </Authorized>
        <NotAuthorized>
            <a class="nav-link" href="Account/Login">
                <span class="bi bi-person-badge-nav-menu" aria-hidden="true"></span> Login
            </a>
        </NotAuthorized>
    </AuthorizeView>
</div>

@code {
    private string? currentUrl;

    protected override void OnInitialized()
    {
        currentUrl = Navigation.Uri;
        Navigation.LocationChanged += OnLocationChanged;
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        currentUrl = Navigation.Uri;
        StateHasChanged();
    }

    public void Dispose() => Navigation.LocationChanged -= OnLocationChanged;
}
```
&nbsp;

I **NavMenu.razor** flyttes de menu punkter, der kræver login, ind under `<AuthorizeView>`:
```xml
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
            <div class="nav-item px-3">
                <NavLink class="nav-link" href="weather">
                    <span class="bi bi-list-nested-nav-menu" aria-hidden="true"></span> Weather
                </NavLink>
            </div>
            <div class="nav-item px-3">
                <NavLink class="nav-link" href="user-claims">
                    <span class="bi bi-list-nested-nav-menu" aria-hidden="true"></span> User Claims
                </NavLink>
            </div>
        </AuthorizeView>
    </nav>
</div>
```