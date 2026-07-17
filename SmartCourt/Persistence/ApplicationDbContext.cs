using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Entities;
using SmartCourt.Features.Auth;
using SmartCourt.Persistence.EntitiesConfigurations;

namespace SmartCourt.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) :
    IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all entity configurations in the assembly
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public DbSet<TestEntity> TestEntities { get; set; }
    public DbSet<SampleEntity> SampleEntities { get; set; }
    public DbSet<StoredFile> StoredFiles { get; set; }
    public DbSet<UserVerificationDocument> UserVerificationDocuments { get; set; }

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
