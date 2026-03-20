using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PsicoFinance.Infrastructure.MultiTenancy;
using PsicoFinance.Infrastructure.Persistence;

namespace PsicoFinance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // ── Multi-tenancy ────────────────────────────────────────
        services.AddScoped<ITenantProvider, TenantProvider>();
        services.AddScoped<ITenantResolver, TenantResolver>();
        services.AddScoped<TenantSchemaService>();

        // ── EF Core + PostgreSQL ─────────────────────────────────
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    npgsql.EnableRetryOnFailure(3);
                });
        });

        return services;
    }
}
