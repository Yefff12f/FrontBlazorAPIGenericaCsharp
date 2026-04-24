using FrontBlazor_AppiGenericaCsharp.Components;
using FrontBlazor_AppiGenericaCsharp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

var builder = WebApplication.CreateBuilder(args);

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

// 🔹 HttpClient
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://localhost:5018")
});

// 🔹 ApiService
builder.Services.AddScoped<ApiService>(sp =>
    new ApiService(
        sp.GetRequiredService<HttpClient>(),
        sp.GetRequiredService<AuthenticationService>(),
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