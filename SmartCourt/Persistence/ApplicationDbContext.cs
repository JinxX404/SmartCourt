using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Entities;
using SmartCourt.Features.Auth;

namespace SmartCourt.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) :
    IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<TestEntity> TestEntities { get; set; }
    public DbSet<SampleEntity> SampleEntities { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply entity configurations
        builder.ApplyConfiguration(new UserConfiguration());
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<SmartCourt.Common.AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = System.DateTime.UtcNow;
                    entry.Entity.CreatedBy = "System"; // Placeholder for now
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = System.DateTime.UtcNow;
                    entry.Entity.LastModifiedBy = "System"; // Placeholder for now
                    break;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
