using Auth0.AspNetCore.Authentication;
using BlazorWebAppAuto;
using BlazorWebAppAuto.Components;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

// 👇 new code
builder.Services
    .AddAuth0WebAppAuthentication(options =>
    {
        options.Domain = builder.Configuration["Auth0:Domain"]!;
        options.ClientId = builder.Configuration["Auth0:ClientId"]!;
    });

builder.Services.AddAuthorization();
// 👆 new code

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization(); // new code

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorWebAppAuto.Client._Imports).Assembly);

// 👇 new code
app.MapGroup("/Account").MapLoginAndLogout();
// 👆 new code

app.Run();

