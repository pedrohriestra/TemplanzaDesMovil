using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace Templanza.Mobile.Services;

public class AuthService
{
    private readonly HttpClient _http;
    public AuthService(HttpClient http) => _http = http;

    // ---- LOGIN ----
    public async Task<bool> LoginAsync(string email, string password)
    {
        var msg = await LoginWithMessageAsync(email, password);
        return string.IsNullOrEmpty(msg);
    }

    // Devuelve "" si OK; si falla, mensaje para mostrar en UI
    public async Task<string> LoginWithMessageAsync(string email, string password)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/auth/login", new { email, password });

            if (res.IsSuccessStatusCode)
            {
                var json = await res.Content.ReadFromJsonAsync<TokenDto>();
                if (json == null || string.IsNullOrWhiteSpace(json.Token))
                    return "El servidor no devolvió token.";

                await SecureStorage.SetAsync("jwt", json.Token);
                _http.DefaultRequestHeaders.Authorization = new("Bearer", json.Token);
                return string.Empty;
            }

            // Manejo de errores comunes
            if (res.StatusCode == HttpStatusCode.Unauthorized)
                return "Credenciales inválidas (email o contraseña).";

            if (res.StatusCode == HttpStatusCode.BadRequest)
            {
                var body = await SafeReadAsync(res);
                return string.IsNullOrWhiteSpace(body) ? "Solicitud inválida." : body;
            }

            var fallback = await SafeReadAsync(res);
            return $"Error {(int)res.StatusCode}: {res.ReasonPhrase}. {fallback}";
        }
        catch (HttpRequestException)
        {
            return "No se pudo conectar con la API (revisá tu red o que el servidor esté arriba).";
        }
        catch (Exception ex)
        {
            return $"Error inesperado: {ex.Message}";
        }
    }

    // ---- REGISTER ----
    public async Task<string> RegisterAsync(string nombre, string email, string password)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/auth/register", new { nombre, email, password});
            if (res.IsSuccessStatusCode) return string.Empty;

            if (res.StatusCode == HttpStatusCode.BadRequest)
            {
                var body = await SafeReadAsync(res);
                if (!string.IsNullOrWhiteSpace(body) &&
                    body.Contains("Email", StringComparison.OrdinalIgnoreCase))
                    return "Ese email ya está registrado.";
                return string.IsNullOrWhiteSpace(body) ? "Datos inválidos para el registro." : body;
            }

            return $"Error {(int)res.StatusCode}: {res.ReasonPhrase}";
        }
        catch (HttpRequestException)
        {
            return "No se pudo conectar con la API para registrar.";
        }
        catch (Exception ex)
        {
            return $"Error inesperado: {ex.Message}";
        }
    }

    public async Task RestoreTokenAsync()
    {
        try
        {
            var t = await SecureStorage.GetAsync("jwt");
            if (!string.IsNullOrWhiteSpace(t))
                _http.DefaultRequestHeaders.Authorization = new("Bearer", t);
        }
        catch { /* noop */ }
    }

    public async Task LogoutAsync()
    {
        try
        {
            await SecureStorage.SetAsync("jwt", "");
            _http.DefaultRequestHeaders.Authorization = null;
        }
        catch { /* noop */ }
    }

    private static async Task<string> SafeReadAsync(HttpResponseMessage res)
    {
        try { return await res.Content.ReadAsStringAsync(); }
        catch { return ""; }
    }

    private record TokenDto(string Token);

    public bool IsLoggedIn => _http.DefaultRequestHeaders.Authorization is not null;

    public async Task<bool> IsAdminAsync()
    {
        await RestoreTokenAsync();
        var role = GetClaim("role");
        return string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<int?> GetUserIdAsync()
    {
        await RestoreTokenAsync();
        var idStr = GetClaim("nameidentifier"); // ClaimTypes.NameIdentifier
        return int.TryParse(idStr, out var id) ? id : null;
    }

    // ---------- helpers JWT ----------
    private string? GetClaim(string key)
    {
        var auth = _http.DefaultRequestHeaders.Authorization;
        if (auth is null || string.IsNullOrWhiteSpace(auth.Parameter)) return null;

        var parts = auth.Parameter.Split('.');
        if (parts.Length < 2) return null;

        try
        {
            var payloadJson = DecodeBase64Url(parts[1]);
            using var doc = System.Text.Json.JsonDocument.Parse(payloadJson);
            var root = doc.RootElement;

            var candidates = key switch
            {
                "role" => new[]
                {
                    "role",
                    "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
                },
                "nameidentifier" => new[]
                {
                    "nameid",
                    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
                    "http://schemas.microsoft.com/ws/2008/06/identity/claims/primarysid"
                },
                _ => new[] { key }
            };

            foreach (var k in candidates)
                if (root.TryGetProperty(k, out var v)) return v.GetString();

            return null;
        }
        catch { return null; }
    }

    private static string DecodeBase64Url(string s)
    {
        s = s.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4) { case 2: s += "=="; break; case 3: s += "="; break; }
        var bytes = Convert.FromBase64String(s);
        return Encoding.UTF8.GetString(bytes);
    }
}
