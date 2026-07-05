using Microsoft.EntityFrameworkCore;
using SmartCourt.Core.Entities;

namespace SmartCourt.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<TestEntity> TestEntities { get; set; }
}
