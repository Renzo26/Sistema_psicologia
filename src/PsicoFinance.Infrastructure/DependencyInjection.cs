using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Infrastructure.MultiTenancy;
using PsicoFinance.Infrastructure.Persistence;
using PsicoFinance.Infrastructure.Services.Audit;
using PsicoFinance.Infrastructure.Services.Auth;
using PsicoFinance.Infrastructure.Services.Encryption;

namespace PsicoFinance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // ── Multi-tenancy ────────────────────────────────────────
        services.AddScoped<Application.Common.Interfaces.ITenantProvider, TenantProvider>();
        services.AddScoped<ITenantResolver, TenantResolver>();
        services.AddScoped<TenantSchemaService>();
        services.AddScoped<ITenantSchemaService>(sp => sp.GetRequiredService<TenantSchemaService>());

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

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        // ── JWT Authentication ───────────────────────────────────
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()!;
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();

        // ── Auth Services ────────────────────────────────────────
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IEmailService, EmailService>();

        // ── Audit ──────────────────────────────────────────────────
        services.AddScoped<IAuditService, AuditService>();

        // ── Encryption (LGPD) ─────────────────────────────────────
        services.AddSingleton<IEncryptionService, AesEncryptionService>();

        return services;
    }
}
