using FrontBlazor_AppiGenericaCsharp.Components;
using FrontBlazor_AppiGenericaCsharp.Services;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios de Blazor Server
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configurar HttpClient para conectarse a la API
// La URL base apunta a la API ApiGenericaCsharp que corre en el puerto 5018
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://localhost:5018")
});

// ✅ LÍNEA AGREGADA: registra ApiService en el contenedor de dependencias
// Sin esto, Blazor no puede inyectar ApiService con @inject en las páginas
builder.Services.AddScoped<ApiService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();