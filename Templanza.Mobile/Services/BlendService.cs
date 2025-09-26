using System.Net.Http.Json;
using Templanza.Domain;

namespace Templanza.Mobile.Services;

public class BlendsService
{
    private readonly HttpClient _http;
    private readonly AuthService _auth;

    public BlendsService(HttpClient http, AuthService auth)
    { _http = http; _auth = auth; }

    public Task<List<Blend>?> GetAllAsync() =>
        _http.GetFromJsonAsync<List<Blend>>("api/blends");

    public async Task<bool> CreateAsync(Blend b)
    {
        await _auth.RestoreTokenAsync();
        var r = await _http.PostAsJsonAsync("api/blends", b);
        return r.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await _auth.RestoreTokenAsync();
        var r = await _http.DeleteAsync($"api/blends/{id}");
        return r.IsSuccessStatusCode;
    }
}
