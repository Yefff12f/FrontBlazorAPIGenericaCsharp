using System.Net.Http.Json;
using System.Text.Json;
using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace FrontBlazor_AppiGenericaCsharp.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;
        private readonly AuthenticationService _authService;
        private readonly RolePermissionService _permissionService;
        private readonly NavigationManager _nav;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private static readonly string[] DateFormats =
        {
            "yyyy-MM-dd",
            "yyyy/MM/dd",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm:ss.FFFFFFF",
            "yyyy-MM-ddTHH:mm:ssZ",
            "yyyy-MM-ddTHH:mm:ss.FFFFFFFZ",
            "dd/MM/yyyy",
            "d/M/yyyy",
            "dd/MM/yyyy HH:mm:ss",
            "d/M/yyyy HH:mm:ss",
            "dd/MM/yyyy hh:mm:ss tt",
            "d/M/yyyy hh:mm:ss tt",
            "MM/dd/yyyy",
            "M/d/yyyy"
        };

        public ApiService(HttpClient http, AuthenticationService authService, RolePermissionService permissionService, NavigationManager nav)
        {
            _http = http;
            _authService = authService;
            _permissionService = permissionService;
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
                    return FiltrarRegistros(tabla, ConvertirDatos(json));
                }

                if (json.TryGetProperty("datos", out JsonElement datos))
                {
                    return FiltrarRegistros(tabla, ConvertirDatos(datos));
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
                if (!_permissionService.CanCreateTable(tabla))
                {
                    return (false, "No tienes permisos para crear en este modulo.");
                }

                if (EsModuloProfesional(tabla) && !_permissionService.IsOwnDocenteRecord(string.Empty, string.Empty, datos))
                {
                    return (false, "Solo puedes crear tus propios datos.");
                }

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
                if (!_permissionService.CanEditTable(tabla))
                {
                    return (false, "No tienes permisos para editar en este modulo.");
                }

                if (EsModuloProfesional(tabla) && !_permissionService.IsOwnDocenteRecord(nombreClave, valorClave, datos))
                {
                    return (false, "Solo puedes editar tus propios datos.");
                }

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
                if (!_permissionService.CanDeleteTable(tabla))
                {
                    return (false, "No tienes permisos para eliminar en este modulo.");
                }

                if (EsModuloProfesional(tabla) && !_permissionService.IsOwnDocenteRecord(nombreClave, valorClave))
                {
                    return (false, "Solo puedes eliminar tus propios datos.");
                }

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
                        JsonValueKind.String => NormalizarString(propiedad.Value.GetString()),
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

        private List<Dictionary<string, object?>> FiltrarRegistros(string tabla, List<Dictionary<string, object?>> registros)
        {
            if (!EsModuloProfesional(tabla) || _permissionService.GetRole() != "docente")
            {
                return registros;
            }

            var docenteId = _permissionService.GetDocenteId();
            if (docenteId == null)
            {
                return registros;
            }

            return registros
                .Where(_permissionService.IsRecordOwnedByCurrentDocente)
                .ToList();
        }

        private static bool EsModuloProfesional(string tabla)
        {
            return tabla.Equals("docente", StringComparison.OrdinalIgnoreCase)
                || tabla.Equals("red", StringComparison.OrdinalIgnoreCase)
                || tabla.Equals("reconocimiento", StringComparison.OrdinalIgnoreCase)
                || tabla.Equals("experiecia", StringComparison.OrdinalIgnoreCase)
                || tabla.Equals("evaluacion_docente", StringComparison.OrdinalIgnoreCase)
                || tabla.Equals("estudios_realizados", StringComparison.OrdinalIgnoreCase)
                || tabla.Equals("beca", StringComparison.OrdinalIgnoreCase)
                || tabla.Equals("apoyo_profesoral", StringComparison.OrdinalIgnoreCase)
                || tabla.Equals("red_docente", StringComparison.OrdinalIgnoreCase)
                || tabla.Equals("estudio_ac", StringComparison.OrdinalIgnoreCase)
                || tabla.Equals("intereses_futuros", StringComparison.OrdinalIgnoreCase);
        }

        private static object? NormalizarString(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return valor;
            }

            return TryNormalizarFecha(valor, out var fechaNormalizada)
                ? fechaNormalizada
                : valor;
        }

        private static bool TryNormalizarFecha(string valor, out string fechaNormalizada)
        {
            fechaNormalizada = string.Empty;
            var texto = valor.Trim();

            if (!PareceFecha(texto))
            {
                return false;
            }

            if (DateTimeOffset.TryParseExact(
                texto,
                DateFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal,
                out var dto))
            {
                fechaNormalizada = dto.Date.ToString("yyyy-MM-dd");
                return true;
            }

            if (DateTime.TryParse(
                texto,
                CultureInfo.GetCultureInfo("es-CO"),
                DateTimeStyles.AllowWhiteSpaces,
                out var fechaEs))
            {
                fechaNormalizada = fechaEs.ToString("yyyy-MM-dd");
                return true;
            }

            if (DateTime.TryParse(
                texto,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces,
                out var fecha))
            {
                fechaNormalizada = fecha.ToString("yyyy-MM-dd");
                return true;
            }

            return false;
        }

        private static bool PareceFecha(string texto)
        {
            return texto.Contains('/') || texto.Contains('-') || texto.Contains('T');
        }
    }
}
