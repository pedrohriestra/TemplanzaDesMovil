using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Templanza.Api.Data;
using Templanza.Domain;

namespace Templanza.Api.Controllers;

public record RegisterDto(string Email, string Password, string? Nombre);
public record LoginDto(string Email, string Password);
public record TokenDto(string Token);

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _cfg;
    public AuthController(AppDbContext db, IConfiguration cfg)
    {
        _db = db; _cfg = cfg;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (await _db.Usuarios.AnyAsync(u => u.Email == dto.Email))
            return BadRequest("Email ya registrado");

        CreatePasswordHash(dto.Password, out var hash, out var salt);

        var isFirstUser = !await _db.Usuarios.AnyAsync(); // si no hay usuarios, este será admin

        var u = new Usuario
        {
            Email = dto.Email,
            Nombre = dto.Nombre, // <- ahora guardamos el nombre
            PasswordHash = hash,
            PasswordSalt = salt,
            Rol = isFirstUser ? RolUsuario.Admin : RolUsuario.Usuario
        };
        _db.Usuarios.Add(u);
        await _db.SaveChangesAsync();
        return Ok();
    }


    [HttpPost("login")]
    public async Task<ActionResult<TokenDto>> Login(LoginDto dto)
    {
        var u = await _db.Usuarios.SingleOrDefaultAsync(x => x.Email == dto.Email);
        if (u is null || !VerifyPassword(dto.Password, u))
            return Unauthorized();

        var token = BuildJwt(u);
        return new TokenDto(token);
    }

    // helpers
    private static void CreatePasswordHash(string pwd, out byte[] hash, out byte[] salt)
    {
        using var h = new HMACSHA512();
        salt = h.Key;
        hash = h.ComputeHash(Encoding.UTF8.GetBytes(pwd));
    }

    private static bool VerifyPassword(string pwd, Usuario u)
    {
        using var h = new HMACSHA512(u.PasswordSalt);
        var computed = h.ComputeHash(Encoding.UTF8.GetBytes(pwd));
        return computed.SequenceEqual(u.PasswordHash);
    }

    private string BuildJwt(Usuario u)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, u.Id.ToString()),
        new Claim(ClaimTypes.Email, u.Email),
        new Claim(ClaimTypes.Role, u.Rol.ToString()) // <<< IMPORTANTE
    };

        var jwt = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

}
