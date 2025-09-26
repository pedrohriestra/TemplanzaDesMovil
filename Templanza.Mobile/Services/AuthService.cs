using System.Net.Http.Json;
using System.Text;
using System;

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
    public async Task<string> LoginWithMessageAsync(string email, string password)
    {
        try
        {
            // log simple
            Console.WriteLine($"AuthService: login attempt for {email}");

            var res = await _http.PostAsJsonAsync("api/auth/login", new { email, password });
            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync();
                var err = $"HTTP {(int)res.StatusCode} - {res.ReasonPhrase}. {body}";
                Console.WriteLine($"AuthService: login failed: {err}");
                return err;
            }

            var json = await res.Content.ReadFromJsonAsync<TokenDto>();
            if (json == null || string.IsNullOrWhiteSpace(json.Token))
            {
                Console.WriteLine("AuthService: token missing in response");
                return "Token no devuelto por el servidor.";
            }

            await SecureStorage.SetAsync("jwt", json.Token);
            _http.DefaultRequestHeaders.Authorization = new("Bearer", json.Token);
            Console.WriteLine("AuthService: login ok, token guardado");
            return string.Empty;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AuthService: exception in Login: {ex}");
            return ex.Message;
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
