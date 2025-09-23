using System.ComponentModel.DataAnnotations;

namespace Templanza.Domain;

public enum RolUsuario { Usuario = 0, Admin = 1 }

public class Usuario
{
    public int Id { get; set; }

    [Required, EmailAddress, StringLength(120)]
    public string Email { get; set; } = "";

    // auth
    public byte[] PasswordHash { get; set; } = default!;
    public byte[] PasswordSalt { get; set; } = default!;

    // campos para ABM (visuales)
    [StringLength(80)]
    public string? Nombre { get; set; }

    public RolUsuario Rol { get; set; } = RolUsuario.Usuario;

    [StringLength(200)]
    public string? ImagenUrl { get; set; } // acá usaremos ruta local (wwwroot)

    public bool Activo { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
