using System.Net.Http.Json;
using System.Text;
using System;
using System.Diagnostics;

namespace Templanza.Mobile.Services;

public class AuthService
{
    private readonly HttpClient _http;
    public AuthService(HttpClient http) => _http = http;

    // Método «tradicional» (mantener compatibilidad)
    public async Task<bool> LoginAsync(string email, string password)
    {
        var msg = await LoginWithMessageAsync(email, password);
        return string.IsNullOrEmpty(msg);
    }

    // Devuelve string vacío si OK, o mensaje de error si falla
    // dentro de AuthService (Mobile)
    public async Task<string> LoginWithMessageAsync(string email, string password)
    {
        try
        {
            Console.WriteLine($"AuthService: login attempt for {email}");

            var res = await _http.PostAsJsonAsync("api/auth/login", new { Email = email, Password = password });
            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();

                if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Console.WriteLine("AuthService: login failed: Unauthorized");
                    return "Usuario o contraseña incorrectos.";
                }

                if (res.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    // Si el backend devuelve un body con detalles, usa eso; si no, mensaje genérico
                    var detail = string.IsNullOrWhiteSpace(body) ? "Solicitud inválida." : body;
                    Console.WriteLine($"AuthService: login bad request: {detail}");
                    return detail;
                }

                // otros errores
                var err = $"Error del servidor ({(int)res.StatusCode}).";
                Console.WriteLine($"AuthService: login failed: {err} {body}");
                return $"{err} Intentá de nuevo más tarde.";
            }

            var json = await res.Content.ReadFromJsonAsync<TokenDto>();
            if (json == null || string.IsNullOrWhiteSpace(json.Token))
            {
                Console.WriteLine("AuthService: token missing in response");
                return "El servidor no devolvió token.";
            }

            await SecureStorage.SetAsync("jwt", json.Token);
            _http.DefaultRequestHeaders.Authorization = new("Bearer", json.Token);
            Console.WriteLine("AuthService: login ok, token guardado");
            return string.Empty;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"AuthService: exception in Login: {ex}");
            Debug.WriteLine($"AuthService: exception in Login: {ex}");
            // mensaje amigable
            return $"{ex}";
        }
    }

    public async Task<string> RegisterAsync(string nombre, string email, string password)
    {
        try
        {
            var payload = new { Email = email, Password = password, Nombre = nombre };
            var res = await _http.PostAsJsonAsync("api/auth/register", payload);
            if (res.IsSuccessStatusCode) return string.Empty;

            var body = await res.Content.ReadAsStringAsync();
            if (res.StatusCode == System.Net.HttpStatusCode.BadRequest && !string.IsNullOrWhiteSpace(body))
                return body;

            return $"Error ({(int)res.StatusCode}) al registrar.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Register exception: {ex}");
            return "Error de conexión al registrar.";
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
        catch (Exception ex)
        {
            Console.WriteLine($"AuthService.RestoreTokenAsync exception: {ex.Message}");
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            await SecureStorage.SetAsync("jwt", "");
            _http.DefaultRequestHeaders.Authorization = null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AuthService.LogoutAsync exception: {ex.Message}");
        }
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

    // ---------- helpers ----------
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
