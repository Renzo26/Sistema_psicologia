using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Domain.Entities;

namespace PsicoFinance.Infrastructure.Services.Audit;

public class AuditService : IAuditService
{
    private readonly IAppDbContext _context;

    public AuditService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task RegistrarAsync(AuditLog log, CancellationToken ct = default)
    {
        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync(ct);
    }
}
