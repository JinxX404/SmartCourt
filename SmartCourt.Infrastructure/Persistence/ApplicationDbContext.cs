using Microsoft.EntityFrameworkCore;
using SmartCourt.Core.Entities;

namespace SmartCourt.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<TestEntity> TestEntities { get; set; }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<SmartCourt.Core.Common.AuditableEntity>())
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
