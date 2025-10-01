using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Templanza.Api.Data;
using Templanza.Domain;

namespace Templanza.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
public class UsuariosController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _cfg;

    public UsuariosController(AppDbContext db, IConfiguration cfg)
    {
        _db = db;
        _cfg = cfg;
    }

    // DTOs públicos
    public record UsuarioDto(int Id, string Email, string? Nombre, string Rol, string? ImagenUrl, bool Activo, DateTime CreatedAt);
    public record CreateUsuarioDto(string Email, string? Nombre, string? Password, string? ImagenUrl, bool Activo, string Rol = "Usuario");
    public record UpdateUsuarioDto(string? Email, string? Nombre, string? Password, string? ImagenUrl, bool? Activo);

    // Helper: map entity -> dto (rol como string)
    private static UsuarioDto ToDto(Usuario u) =>
        new UsuarioDto(u.Id, u.Email, u.Nombre, u.Rol.ToString(), u.ImagenUrl, u.Activo, u.CreatedAt);

    // ----------------- ADMIN -----------------

    // GET api/usuarios
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<UsuarioDto>>> GetAll()
    {
        var list = await _db.Usuarios
            .OrderByDescending(u => u.Id)
            .ToListAsync();

        return list.Select(ToDto).ToList();
    }

    // GET api/usuarios/{id}
    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<UsuarioDto>> GetById(int id)
    {
        var myId = GetUserId();
        if (!IsAdmin() && myId != id) return Forbid();

        var u = await _db.Usuarios.FindAsync(id);
        if (u == null) return NotFound();
        return ToDto(u);
    }

    // POST api/usuarios  (Admin crea usuarios)
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UsuarioDto>> Create(CreateUsuarioDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest("Email requerido.");

        if (await _db.Usuarios.AnyAsync(x => x.Email == dto.Email))
            return BadRequest("Email ya registrado.");

        if (string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Password requerido al crear usuario.");

        CreatePasswordHash(dto.Password, out var hash, out var salt);

        // parse role (fallback to Usuario)
        var role = RolUsuario.Usuario;
        if (!string.IsNullOrWhiteSpace(dto.Rol))
        {
            Enum.TryParse<RolUsuario>(dto.Rol, true, out role);
        }

        var u = new Usuario
        {
            Email = dto.Email,
            Nombre = dto.Nombre,
            ImagenUrl = dto.ImagenUrl,
            Activo = dto.Activo,
            PasswordHash = hash,
            PasswordSalt = salt,
            Rol = role,
            CreatedAt = DateTime.UtcNow
        };

        _db.Usuarios.Add(u);
        await _db.SaveChangesAsync();

        var result = ToDto(u);
        return CreatedAtAction(nameof(GetById), new { id = u.Id }, result);
    }

    // PUT api/usuarios/{id}  (Admin actualiza cualquiera)
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateAny(int id, UpdateUsuarioDto dto)
    {
        var u = await _db.Usuarios.FindAsync(id);
        if (u == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(dto.Email)) u.Email = dto.Email;
        if (!string.IsNullOrWhiteSpace(dto.Nombre)) u.Nombre = dto.Nombre;
        if (!string.IsNullOrWhiteSpace(dto.ImagenUrl)) u.ImagenUrl = dto.ImagenUrl;
        if (dto.Activo.HasValue) u.Activo = dto.Activo.Value;

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            CreatePasswordHash(dto.Password, out var hash, out var salt);
            u.PasswordHash = hash;
            u.PasswordSalt = salt;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE api/usuarios/{id}
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var u = await _db.Usuarios.FindAsync(id);
        if (u == null) return NotFound();

        _db.Usuarios.Remove(u);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ----------------- SELF (perfil) -----------------

    // GET api/usuarios/me
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UsuarioDto>> GetMe()
    {
        var id = GetUserId();
        var u = await _db.Usuarios.FindAsync(id);
        if (u == null) return NotFound();
        return ToDto(u);
    }

    // PUT api/usuarios/me
    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateMe(UpdateUsuarioDto dto)
    {
        var id = GetUserId();
        var u = await _db.Usuarios.FindAsync(id);
        if (u == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(dto.Email)) u.Email = dto.Email;
        if (!string.IsNullOrWhiteSpace(dto.Nombre)) u.Nombre = dto.Nombre;
        if (!string.IsNullOrWhiteSpace(dto.ImagenUrl)) u.ImagenUrl = dto.ImagenUrl;

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            CreatePasswordHash(dto.Password, out var hash, out var salt);
            u.PasswordHash = hash;
            u.PasswordSalt = salt;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ----------------- helpers -----------------
    private int GetUserId()
    {
        var v = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(v, out var id) ? id : 0;
    }

    private bool IsAdmin() => User.IsInRole("Admin");

    private static void CreatePasswordHash(string pwd, out byte[] hash, out byte[] salt)
    {
        using var h = new HMACSHA512();
        salt = h.Key;
        hash = h.ComputeHash(Encoding.UTF8.GetBytes(pwd));
    }
}
