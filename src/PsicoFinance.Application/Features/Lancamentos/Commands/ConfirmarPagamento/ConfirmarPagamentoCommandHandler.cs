using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Lancamentos.Commands.CriarLancamento;
using PsicoFinance.Application.Features.Lancamentos.DTOs;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Lancamentos.Commands.ConfirmarPagamento;

public class ConfirmarPagamentoCommandHandler : IRequestHandler<ConfirmarPagamentoCommand, LancamentoDto>
{
    private readonly IAppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public ConfirmarPagamentoCommandHandler(IAppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<LancamentoDto> Handle(ConfirmarPagamentoCommand request, CancellationToken cancellationToken)
    {
        _ = _tenantProvider.ClinicaId
            ?? throw new UnauthorizedAccessException("Tenant não identificado.");

        var lancamento = await _context.LancamentosFinanceiros
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Lançamento não encontrado.");

        if (lancamento.Status == StatusLancamento.Cancelado)
            throw new InvalidOperationException("Não é possível confirmar um lançamento cancelado.");

        if (lancamento.Status == StatusLancamento.Confirmado)
            throw new InvalidOperationException("Lançamento já está confirmado.");

        lancamento.Status = StatusLancamento.Confirmado;
        lancamento.DataPagamento = request.DataPagamento;

        await _context.SaveChangesAsync(cancellationToken);

        var planoConta = await _context.PlanosConta
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == lancamento.PlanoContaId, cancellationToken);

        return CriarLancamentoCommandHandler.MapToDto(lancamento, planoConta?.Nome ?? string.Empty);
    }
}
