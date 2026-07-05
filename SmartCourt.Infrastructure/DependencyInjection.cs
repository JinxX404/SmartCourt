using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SmartCourt.Infrastructure.Persistence;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            
        // In the future, we will register Repositories, Identity, and Email services here
            
        return services;
    }
}
