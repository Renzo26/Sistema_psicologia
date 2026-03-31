using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Lancamentos.Commands.CriarLancamento;
using PsicoFinance.Application.Features.Lancamentos.DTOs;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Lancamentos.Commands.AtualizarLancamento;

public class AtualizarLancamentoCommandHandler : IRequestHandler<AtualizarLancamentoCommand, LancamentoDto>
{
    private readonly IAppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public AtualizarLancamentoCommandHandler(IAppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<LancamentoDto> Handle(AtualizarLancamentoCommand request, CancellationToken cancellationToken)
    {
        _ = _tenantProvider.ClinicaId
            ?? throw new UnauthorizedAccessException("Tenant não identificado.");

        var lancamento = await _context.LancamentosFinanceiros
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Lançamento não encontrado.");

        if (lancamento.Status == StatusLancamento.Cancelado)
            throw new InvalidOperationException("Não é possível editar um lançamento cancelado.");

        var periodoFechado = await _context.FechamentosMensais
            .AnyAsync(f => f.MesReferencia == lancamento.Competencia
                        && f.Status == StatusFechamento.Fechado, cancellationToken);
        if (periodoFechado)
            throw new InvalidOperationException($"O período {lancamento.Competencia} está fechado e não permite edições.");

        var planoConta = await _context.PlanosConta
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.PlanoContaId, cancellationToken)
            ?? throw new KeyNotFoundException("Plano de conta não encontrado.");

        lancamento.Descricao = request.Descricao;
        lancamento.Valor = request.Valor;
        lancamento.Tipo = request.Tipo;
        lancamento.DataVencimento = request.DataVencimento;
        lancamento.Competencia = request.Competencia;
        lancamento.PlanoContaId = request.PlanoContaId;
        lancamento.Observacao = request.Observacao;

        await _context.SaveChangesAsync(cancellationToken);

        return CriarLancamentoCommandHandler.MapToDto(lancamento, planoConta.Nome);
    }
}
