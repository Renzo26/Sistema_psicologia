using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;

namespace PsicoFinance.Application.Features.PlanosConta.Commands.ExcluirPlanoConta;

public class ExcluirPlanoContaCommandHandler : IRequestHandler<ExcluirPlanoContaCommand>
{
    private readonly IAppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public ExcluirPlanoContaCommandHandler(IAppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(ExcluirPlanoContaCommand request, CancellationToken cancellationToken)
    {
        _ = _tenantProvider.ClinicaId
            ?? throw new UnauthorizedAccessException("Tenant não identificado.");

        var plano = await _context.PlanosConta
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Plano de conta não encontrado.");

        var emUso = await _context.Contratos
            .AnyAsync(c => c.PlanoContaId == request.Id, cancellationToken);

        if (emUso)
            throw new InvalidOperationException("Não é possível excluir um plano de conta vinculado a contratos.");

        _context.PlanosConta.Remove(plano);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
