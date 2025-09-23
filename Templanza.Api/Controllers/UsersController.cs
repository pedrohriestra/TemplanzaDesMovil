using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Templanza.Api.Data;
using Templanza.Domain;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    public UsersController(AppDbContext db) => _db = db;

    // Solo Admin: listar todos
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IEnumerable<Usuario>> GetAll() =>
        await _db.Usuarios.OrderByDescending(u => u.Id).ToListAsync();

    // Self: ver mis datos
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<Usuario>> GetMe()
    {
        var id = GetUserId();
        var u = await _db.Usuarios.FindAsync(id);
        return u is null ? NotFound() : u;
    }

    // Admin o Self: ver por id (usuario común no puede ver a otros)
    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Usuario>> GetById(int id)
    {
        var myId = GetUserId();
        if (!IsAdmin() && id != myId) return Forbid();

        var u = await _db.Usuarios.FindAsync(id);
        return u is null ? NotFound() : u;
    }

    // Self: actualizar SU cuenta (campos acotados)
    public record UsuarioSelfDto(string? Nombre, string? ImagenUrl);

    [Authorize]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe(UsuarioSelfDto dto)
    {
        var id = GetUserId();
        var u = await _db.Usuarios.FindAsync(id);
        if (u is null) return NotFound();

        u.Nombre = dto.Nombre;
        u.ImagenUrl = dto.ImagenUrl;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // Admin: actualizar cualquier usuario (campos completos de ABM)
    public record UsuarioAdminDto(string Email, string? Nombre, RolUsuario Rol, string? ImagenUrl, bool Activo);

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAny(int id, UsuarioAdminDto dto)
    {
        var u = await _db.Usuarios.FindAsync(id);
        if (u is null) return NotFound();

        u.Email = dto.Email;
        u.Nombre = dto.Nombre;
        u.Rol = dto.Rol;
        u.ImagenUrl = dto.ImagenUrl;
        u.Activo = dto.Activo;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // Admin: borrar
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var u = await _db.Usuarios.FindAsync(id);
        if (u is null) return NotFound();
        _db.Usuarios.Remove(u);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // Helpers (pueden ir privados dentro del controller)
    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool IsAdmin() => User.IsInRole("Admin");
}
