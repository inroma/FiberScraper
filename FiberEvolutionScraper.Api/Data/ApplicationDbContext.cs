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
            .HasKey(p => p.Signature);
        modelBuilder.Entity<FiberPointDTO>()
            .HasIndex(p => p.Signature)
            .IsUnique(true);
        modelBuilder.Entity<FiberPointDTO>()
            .HasMany(c => c.EligibilitesFtth)
            .WithOne()
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<FiberPointDTO>().Navigation(e => e.EligibilitesFtth).AutoInclude();

        modelBuilder.Entity<FiberPointDTO>()
            .Property(b => b.Created)
            .IsRequired();

        modelBuilder.Entity<FiberPointDTO>()
            .Property(b => b.LastUpdated)
            .IsRequired();


        modelBuilder.Entity<EligibiliteFtthDTO>()
            .HasKey(p => new { p.CodeImb, p.EtapeFtth });
        modelBuilder.Entity<EligibiliteFtthDTO>()
            .HasIndex(p => new { p.CodeImb, p.EtapeFtth })
            .IsUnique(true);

        modelBuilder.Entity<EligibiliteFtthDTO>()
            .Property(b => b.Created)
            .IsRequired();

        modelBuilder.Entity<EligibiliteFtthDTO>()
            .Property(b => b.LastUpdated)
            .IsRequired();
    }

    public override int SaveChanges()
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is BaseModelDTO && (
                    e.State == EntityState.Added
                    || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            ((BaseModelDTO)entityEntry.Entity).LastUpdated = DateTime.UtcNow;

            if (entityEntry.State == EntityState.Added)
            {
                ((BaseModelDTO)entityEntry.Entity).Created = DateTime.UtcNow;
            }
        }

        return base.SaveChanges();
    }
}
