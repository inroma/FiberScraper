using FiberEvolutionScraper.Api.Models;
using FiberEvolutionScraper.Api.Models.User;
using Microsoft.EntityFrameworkCore;

namespace FiberEvolutionScraper.Api.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<FiberPoint> FiberPoints { get; set; }

    public DbSet<EligibiliteFtth> EligibiliteFtth { get; set; }

    public DbSet<AutoRefreshInput> AutoRefreshInputs { get; set; }

    public DbSet<UserModel> Users { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)
    {
        Database.EnsureCreated();
        ChangeTracker.LazyLoadingEnabled = false;
    }

    public ApplicationDbContext()
    {
        Database.EnsureCreated();
        ChangeTracker.LazyLoadingEnabled = false;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FiberPoint>()
            .HasIndex(p => p.Signature)
            .IsUnique(true);
        modelBuilder.Entity<FiberPoint>()
            .HasMany(c => c.EligibilitesFtth)
            .WithOne()
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<FiberPoint>().Navigation(e => e.EligibilitesFtth).AutoInclude();

        modelBuilder.Entity<EligibiliteFtth>()
            .HasOne(e => e.FiberPoint)
            .WithMany(b => b.EligibilitesFtth)
            .HasForeignKey(e => e.FiberPointDTOSignature);

        modelBuilder.Entity<UserModel>()
            .HasMany(u => u.AutoRefreshs)
            .WithOne(a => a.User)
            .HasForeignKey(a => a.UserId);

        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is BaseModel && (e.State == EntityState.Added));

        foreach (var entityEntry in entries)
        {
            ((BaseModel)entityEntry.Entity).Created = DateTime.UtcNow;
        }

        return base.SaveChanges();
    }
}
