using FiberEvolutionScraper.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FiberEvolutionScraper.Api.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<FiberPointDTO> FiberPoints { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)
    {
        Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FiberPointDTO>()
            .HasKey(p => new { p.Signature, p.EtapeFtth });
        modelBuilder.Entity<FiberPointDTO>()
            .HasIndex(p => new { p.Signature, p.EtapeFtth })
        .IsUnique(true);

        modelBuilder.Entity<FiberPointDTO>()
            .Property(b => b.Created)
            .IsRequired();

        modelBuilder.Entity<FiberPointDTO>()
            .Property(b => b.LastUpdated)
            .IsRequired();
    }
}
