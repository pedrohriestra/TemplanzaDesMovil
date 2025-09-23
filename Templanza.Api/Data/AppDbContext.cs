using Microsoft.EntityFrameworkCore;
using Templanza.Domain;

namespace Templanza.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Blend> Blends => Set<Blend>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // Nada especial por ahora.
    }
}
