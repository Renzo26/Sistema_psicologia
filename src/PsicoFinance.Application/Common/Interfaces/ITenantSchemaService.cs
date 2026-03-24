namespace PsicoFinance.Application.Common.Interfaces;

public interface ITenantSchemaService
{
    Task CreateSchemaForTenantAsync(Guid clinicaId, CancellationToken ct = default);
    Task<bool> SchemaExistsAsync(Guid clinicaId, CancellationToken ct = default);
}
