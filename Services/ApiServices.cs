using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components;

namespace FrontBlazor_AppiGenericaCsharp.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;
        private readonly AuthenticationService _authService;
        private readonly NavigationManager _nav;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiService(HttpClient http, AuthenticationService authService, NavigationManager nav)
        {
            _http = http;
            _authService = authService;
            _nav = nav;
        }

        private async Task CheckUnauthorizedAsync(HttpResponseMessage respuesta)
        {
            if (respuesta.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await _authService.LogoutAsync();
                _nav.NavigateTo("/", forceLoad: true);
            }
        }

        public async Task<List<Dictionary<string, object?>>> ListarAsync(string tabla)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"/api/{tabla}");
                var token = _authService.GetToken();
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                var respuesta = await _http.SendAsync(request);
                
                await CheckUnauthorizedAsync(respuesta);
                
                respuesta.EnsureSuccessStatusCode();
                var json = await respuesta.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);

                if (json.ValueKind == JsonValueKind.Array)
                {
                    return ConvertirDatos(json);
                }

                if (json.TryGetProperty("datos", out JsonElement datos))
                {
                    return ConvertirDatos(datos);
                }

                return new List<Dictionary<string, object?>>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error al listar {tabla}: {ex.Message}");
                return new List<Dictionary<string, object?>>();
            }
        }

        public async Task<(bool exito, string mensaje)> CrearAsync(
            string tabla, Dictionary<string, object?> datos)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"/api/{tabla}");
                var token = _authService.GetToken();
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                request.Content = JsonContent.Create(datos);
                var respuesta = await _http.SendAsync(request);
                
                await CheckUnauthorizedAsync(respuesta);
                
                var contenido = await respuesta.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);

                string mensaje = contenido.TryGetProperty("mensaje", out JsonElement msg)
                    ? msg.GetString() ?? "Operacion completada."
                    : "Operacion completada.";

                return (respuesta.IsSuccessStatusCode, mensaje);
            }
            catch (HttpRequestException ex)
            {
                return (false, $"Error de conexion: {ex.Message}");
            }
        }

        public async Task<(bool exito, string mensaje)> ActualizarAsync(
            string tabla, string nombreClave, string valorClave,
            Dictionary<string, object?> datos)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Put, $"/api/{tabla}/{nombreClave}/{valorClave}");
                var token = _authService.GetToken();
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                request.Content = JsonContent.Create(datos);
                var respuesta = await _http.SendAsync(request);
                
                await CheckUnauthorizedAsync(respuesta);
                
                var contenido = await respuesta.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);

                string mensaje = contenido.TryGetProperty("mensaje", out JsonElement msg)
                    ? msg.GetString() ?? "Operacion completada."
                    : "Operacion completada.";

                return (respuesta.IsSuccessStatusCode, mensaje);
            }
            catch (HttpRequestException ex)
            {
                return (false, $"Error de conexion: {ex.Message}");
            }
        }

        public async Task<(bool exito, string mensaje)> EliminarAsync(
            string tabla, string nombreClave, string valorClave)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/{tabla}/{nombreClave}/{valorClave}");
                var token = _authService.GetToken();
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
                var respuesta = await _http.SendAsync(request);
                
                await CheckUnauthorizedAsync(respuesta);
                
                var contenido = await respuesta.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);

                string mensaje = contenido.TryGetProperty("mensaje", out JsonElement msg)
                    ? msg.GetString() ?? "Operacion completada."
                    : "Operacion completada.";

                return (respuesta.IsSuccessStatusCode, mensaje);
            }
            catch (HttpRequestException ex)
            {
                return (false, $"Error de conexion: {ex.Message}");
            }
        }
        
 public async Task<(bool exito, string? mensaje, System.Text.Json.JsonElement data)> PostAsync(string url, object datos)
{
    var http = new HttpClient();
    http.BaseAddress = new Uri("http://localhost:5018/"); // ajusta puerto

    var response = await http.PostAsJsonAsync(url, datos);

    var content = await response.Content.ReadAsStringAsync();

    // 🔥 SI FALLA → NO INTENTES PARSEAR JSON
    if (!response.IsSuccessStatusCode)
    {
        return (false, content, default);
    }

    // 🔥 SI VIENE VACÍO → EVITAR ERROR
    if (string.IsNullOrWhiteSpace(content))
    {
        return (false, "Respuesta vacía del servidor", default);
    }

    try
    {
        var json = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(content);
        return (true, null, json);
    }
    catch
    {
        return (false, "Respuesta inválida del servidor", default);
    }
}

        // ──────────────────────────────────────────────
        // METODO AUXILIAR: Convierte JsonElement a lista de diccionarios
        // La API devuelve los datos como JSON generico, este metodo
        // lo transforma a Dictionary<string, object?> para trabajar
        // facilmente con @foreach y @bind en Blazor
        // ──────────────────────────────────────────────
        private List<Dictionary<string, object?>> ConvertirDatos(JsonElement datos)
        {
            var lista = new List<Dictionary<string, object?>>();

            foreach (var fila in datos.EnumerateArray())
            {
                var diccionario = new Dictionary<string, object?>();

                foreach (var propiedad in fila.EnumerateObject())
                {
                    diccionario[propiedad.Name] = propiedad.Value.ValueKind switch
                    {
                        JsonValueKind.String => propiedad.Value.GetString(),
                        JsonValueKind.Number => propiedad.Value.TryGetInt32(out int i) ? i : propiedad.Value.GetDouble(),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        JsonValueKind.Null => null,
                        _ => propiedad.Value.GetRawText()
                    };
                }

                lista.Add(diccionario);
            }

            return lista;
        }
    }
}