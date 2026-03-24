using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Infrastructure.Persistence;

namespace PsicoFinance.Infrastructure.MultiTenancy;

public partial class TenantSchemaService : ITenantSchemaService
{
    private readonly AppDbContext _db;
    private readonly ILogger<TenantSchemaService> _logger;

    public TenantSchemaService(AppDbContext db, ILogger<TenantSchemaService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task CreateSchemaForTenantAsync(Guid clinicaId, CancellationToken ct = default)
    {
        var schemaName = SanitizeSchemaName(clinicaId);

        _logger.LogInformation("Criando schema {Schema} para clínica {ClinicaId}", schemaName, clinicaId);

        // Schema name é derivado de um Guid, então é seguro para uso direto
        #pragma warning disable EF1002
        await _db.Database.ExecuteSqlRawAsync(
            $"CREATE SCHEMA IF NOT EXISTS \"{schemaName}\"", ct);
        #pragma warning restore EF1002

        _logger.LogInformation("Schema {Schema} criado com sucesso", schemaName);
    }

    public async Task<bool> SchemaExistsAsync(Guid clinicaId, CancellationToken ct = default)
    {
        var schemaName = SanitizeSchemaName(clinicaId);

        var result = await _db.Database
            .SqlQueryRaw<string>(
                "SELECT schema_name FROM information_schema.schemata WHERE schema_name = {0}",
                schemaName)
            .FirstOrDefaultAsync(ct);

        return result is not null;
    }

    private static string SanitizeSchemaName(Guid clinicaId)
    {
        var name = $"tenant_{clinicaId:N}";
        // Garante que o nome contém apenas alfanuméricos e underscore
        if (!SafeSchemaName().IsMatch(name))
            throw new ArgumentException("Nome de schema inválido", nameof(clinicaId));
        return name;
    }

    [GeneratedRegex("^[a-z0-9_]+$")]
    private static partial Regex SafeSchemaName();
}
