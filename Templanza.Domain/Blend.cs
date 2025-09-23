using System.ComponentModel.DataAnnotations;

namespace Templanza.Domain;

public class Blend
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Nombre { get; set; } = "";

    [StringLength(60)]
    public string? Tipo { get; set; }

    [Range(0, 100000)]
    public decimal Precio { get; set; }

    [Range(0, 100000)]
    public int Stock { get; set; }

    [Display(Name = "Imagen"), StringLength(200)]
    public string? ImagenUrl { get; set; }
}
