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
        foreach (var entry in ChangeTracker.Entries<SmartCourt.Core.Common.BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = System.DateTime.UtcNow;
                    if (entry.Entity is SmartCourt.Core.Common.AuditableEntity auditableAdded)
                    {
                        auditableAdded.CreatedBy = "System"; // Placeholder for now
                    }
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = System.DateTime.UtcNow;
                    if (entry.Entity is SmartCourt.Core.Common.AuditableEntity auditableModified)
                    {
                        auditableModified.LastModifiedBy = "System"; // Placeholder for now
                    }
                    break;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
