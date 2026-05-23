using FrontBlazor_AppiGenericaCsharp.Components;
using FrontBlazor_AppiGenericaCsharp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "5100";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var apiBaseUrl =
    Environment.GetEnvironmentVariable("API_BASE_URL")
    ?? builder.Configuration["Api:BaseUrl"]
    ?? "https://proyecto-servicios-y-aplicaciones-web-production.up.railway.app/";

// 🔹 Blazor Server
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 🔐 AUTH (ESTE ES EL BUENO)
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/login"; // 👈 ESTA ES LA CLAVE
    });

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

// 🔹 Provider
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<CustomAuthenticationStateProvider>());

// 🔹 Auth Service
builder.Services.AddScoped<AuthenticationService>(sp =>
    new AuthenticationService(
        sp.GetRequiredService<CustomAuthenticationStateProvider>(),
        sp.GetRequiredService<IJSRuntime>()
    ));

builder.Services.AddScoped<RolePermissionService>();

// 🔹 HttpClient
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
});

// 🔹 ApiService
builder.Services.AddScoped<ApiService>(sp =>
    new ApiService(
        sp.GetRequiredService<HttpClient>(),
        sp.GetRequiredService<AuthenticationService>(),
        sp.GetRequiredService<RolePermissionService>(),
        sp.GetRequiredService<NavigationManager>()
    ));

var app = builder.Build();

// 🔹 Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
