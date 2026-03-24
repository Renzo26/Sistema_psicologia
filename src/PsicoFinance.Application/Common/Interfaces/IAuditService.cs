using PsicoFinance.Domain.Entities;

namespace PsicoFinance.Application.Common.Interfaces;

public interface IAuditService
{
    Task RegistrarAsync(AuditLog log, CancellationToken ct = default);
}
