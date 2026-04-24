using Microsoft.JSInterop;
using System.Security.Claims;

namespace FrontBlazor_AppiGenericaCsharp.Services
{
    public class AuthenticationService
    {
        private string? _token;
        private readonly CustomAuthenticationStateProvider _authStateProvider;
        private readonly IJSRuntime _js;

        public event Action? OnAuthenticationChanged;

        public AuthenticationService(
            CustomAuthenticationStateProvider authStateProvider,
            IJSRuntime js)
        {
            _authStateProvider = authStateProvider;
            _js = js;
        }

        // 🔥 RESTAURA TOKEN AL INICIAR
        public async Task InitializeAsync()
        {
            _token = await _js.InvokeAsync<string>("localStorage.getItem", "token");

            if (!string.IsNullOrEmpty(_token))
            {
                _authStateProvider.MarkUserAsAuthenticated(_token);
            }
        }

        public string? GetToken() => _token;

        public bool IsAuthenticated() => !string.IsNullOrEmpty(_token);

        // 🔐 GUARDA EN LOCALSTORAGE
        public async Task SetTokenAsync(string token)
        {
            _token = token;

            await _js.InvokeVoidAsync("localStorage.setItem", "token", token);

            _authStateProvider.MarkUserAsAuthenticated(token);
            NotifyAuthenticationChanged();
        }

        public async Task LogoutAsync()
        {
            _token = null;

            await _js.InvokeVoidAsync("localStorage.removeItem", "token");

            _authStateProvider.MarkUserAsLoggedOut();
            NotifyAuthenticationChanged();
        }

        private void NotifyAuthenticationChanged()
        {
            OnAuthenticationChanged?.Invoke();
        }

        public string GetRol()
        {
            if (string.IsNullOrEmpty(_token)) return "";

            var payload = _token.Split('.')[1];
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            var jsonBytes = Convert.FromBase64String(payload);

            var keyValuePairs = System.Text.Json.JsonSerializer
                .Deserialize<Dictionary<string, System.Text.Json.JsonElement>>(jsonBytes);

            if (keyValuePairs != null &&
                keyValuePairs.TryGetValue(ClaimTypes.Role, out var role))
                return role.GetString() ?? "";

            return "";
        }
    }
}