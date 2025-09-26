using System.Net.Http.Json;

namespace Templanza.Mobile.Services;

public class UsersService
{
    private readonly HttpClient _http;
    private readonly AuthService _auth;

    public UsersService(HttpClient http, AuthService auth)
    { _http = http; _auth = auth; }

    // DTOs MUTABLES (para @bind)
    public class UserDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Email { get; set; } = "";
        public string Rol { get; set; } = "Usuario";
        public string? ImagenUrl { get; set; }
        public bool Activo { get; set; } = true;
    }

    public class UserUpsertDto
    {
        public string Nombre { get; set; } = "";
        public string Email { get; set; } = "";
        public string Rol { get; set; } = "Usuario";
        public string? ImagenUrl { get; set; }
        public bool Activo { get; set; } = true;
        public string? Password { get; set; }
    }

    // ----- ADMIN -----
    public async Task<List<UserDto>?> GetAllAsync()
    {
        await _auth.RestoreTokenAsync();
        return await _http.GetFromJsonAsync<List<UserDto>>("api/usuarios");
    }

    public async Task<bool> CreateAsync(UserUpsertDto dto)
    {
        await _auth.RestoreTokenAsync();
        var r = await _http.PostAsJsonAsync("api/usuarios", dto);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAsync(int id, UserUpsertDto dto)
    {
        await _auth.RestoreTokenAsync();
        var r = await _http.PutAsJsonAsync($"api/usuarios/{id}", dto);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await _auth.RestoreTokenAsync();
        var r = await _http.DeleteAsync($"api/usuarios/{id}");
        return r.IsSuccessStatusCode;
    }

    // ----- PERFIL -----
    public async Task<UserDto?> GetMeAsync()
    {
        await _auth.RestoreTokenAsync();
        return await _http.GetFromJsonAsync<UserDto>("api/usuarios/me");
    }

    public async Task<bool> UpdateMeAsync(UserUpsertDto dto)
    {
        await _auth.RestoreTokenAsync();
        var r = await _http.PutAsJsonAsync("api/usuarios/me", dto);
        return r.IsSuccessStatusCode;
    }
}
