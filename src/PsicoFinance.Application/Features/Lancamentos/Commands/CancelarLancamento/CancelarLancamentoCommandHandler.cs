using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Lancamentos.Commands.CancelarLancamento;

public class CancelarLancamentoCommandHandler : IRequestHandler<CancelarLancamentoCommand>
{
    private readonly IAppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public CancelarLancamentoCommandHandler(IAppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(CancelarLancamentoCommand request, CancellationToken cancellationToken)
    {
        _ = _tenantProvider.ClinicaId
            ?? throw new UnauthorizedAccessException("Tenant não identificado.");

        var lancamento = await _context.LancamentosFinanceiros
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Lançamento não encontrado.");

        if (lancamento.Status == StatusLancamento.Cancelado)
            throw new InvalidOperationException("Lançamento já está cancelado.");

        if (lancamento.Status == StatusLancamento.Confirmado)
            throw new InvalidOperationException("Não é possível cancelar um lançamento já confirmado. Estorne-o manualmente.");

        var periodoFechado = await _context.FechamentosMensais
            .AnyAsync(f => f.MesReferencia == lancamento.Competencia
                        && f.Status == StatusFechamento.Fechado, cancellationToken);
        if (periodoFechado)
            throw new InvalidOperationException($"O período {lancamento.Competencia} está fechado e não permite edições.");

        lancamento.Status = StatusLancamento.Cancelado;
        if (request.Motivo is not null)
            lancamento.Observacao = request.Motivo;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
