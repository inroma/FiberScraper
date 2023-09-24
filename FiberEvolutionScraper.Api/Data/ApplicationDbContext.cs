using FiberEvolutionScraper.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FiberEvolutionScraper.Api.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<FiberPointDTO> FiberPoints { get; set; }

    public DbSet<EligibiliteFtthDTO> EligibiliteFtth { get; set; }

    public DbSet<AutoRefreshInput> AutoRefreshInputs { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)
    {
        Database.EnsureCreated();
        ChangeTracker.LazyLoadingEnabled = false;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FiberPointDTO>()
            .HasIndex(p => p.Signature)
            .IsUnique(true);
        modelBuilder.Entity<FiberPointDTO>()
            .HasMany(c => c.EligibilitesFtth)
            .WithOne()
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<FiberPointDTO>().Navigation(e => e.EligibilitesFtth).AutoInclude();

        modelBuilder.Entity<EligibiliteFtthDTO>()
            .HasOne(e => e.FiberPoint)
            .WithMany(b => b.EligibilitesFtth)
            .HasForeignKey(e => e.FiberPointDTOSignature);
    }

    public override int SaveChanges()
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is BaseModelDTO && (
                    e.State == EntityState.Added));

        foreach (var entityEntry in entries)
        {
            ((BaseModelDTO)entityEntry.Entity).Created = DateTime.UtcNow;
        }

        return base.SaveChanges();
    }
}
