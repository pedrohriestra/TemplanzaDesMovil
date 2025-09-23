using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Templanza.Api.Data;
using Templanza.Domain;


namespace Templanza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlendsController : ControllerBase
{
    private readonly AppDbContext _db;
    public BlendsController(AppDbContext db) => _db = db;

    // GET públicos
    [AllowAnonymous]
    [HttpGet]
    public async Task<IEnumerable<Blend>> GetAll() =>
        await _db.Blends.OrderByDescending(b => b.Id).ToListAsync();

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ActionResult<Blend>> GetById(int id)
    {
        var b = await _db.Blends.FindAsync(id);
        return b is null ? NotFound() : b;
    }

    // DTO de creación/edición (para no mandar Id desde el cliente)
    public record BlendDto(string Nombre, string? Tipo, decimal Precio, int Stock, string? ImagenUrl);

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<Blend>> Create(BlendDto dto)
    {
        var b = new Blend
        {
            Nombre = dto.Nombre,
            Tipo = dto.Tipo,
            Precio = dto.Precio,
            Stock = dto.Stock,
            ImagenUrl = dto.ImagenUrl
        };
        _db.Blends.Add(b);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = b.Id }, b);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, BlendDto dto)
    {
        var b = await _db.Blends.FindAsync(id);
        if (b is null) return NotFound();

        b.Nombre = dto.Nombre;
        b.Tipo = dto.Tipo;
        b.Precio = dto.Precio;
        b.Stock = dto.Stock;
        b.ImagenUrl = dto.ImagenUrl;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var b = await _db.Blends.FindAsync(id);
        if (b is null) return NotFound();
        _db.Blends.Remove(b);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
