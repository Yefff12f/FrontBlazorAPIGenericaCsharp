using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace FrontBlazor_AppiGenericaCsharp.Services
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(new AuthenticationState(_currentUser));
        }

        public void MarkUserAsAuthenticated(string token)
        {
            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "apiauth");
            _currentUser = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
        }

        public void MarkUserAsLoggedOut()
        {
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];

            // Padding base64
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            var jsonBytes = Convert.FromBase64String(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes);

            if (keyValuePairs == null) return claims;

            // Nombre de usuario
            if (keyValuePairs.TryGetValue(ClaimTypes.Name, out var name))
                claims.Add(new Claim(ClaimTypes.Name, name.GetString() ?? ""));

            // Rol
            if (keyValuePairs.TryGetValue(ClaimTypes.Role, out var role))
                claims.Add(new Claim(ClaimTypes.Role, role.GetString() ?? ""));

            // ID
            if (keyValuePairs.TryGetValue("id", out var id))
                claims.Add(new Claim("id", id.GetString() ?? ""));

            return claims;
        }
    }
}