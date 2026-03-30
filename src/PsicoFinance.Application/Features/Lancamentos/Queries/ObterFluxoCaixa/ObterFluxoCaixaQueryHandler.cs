using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Lancamentos.DTOs;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Lancamentos.Queries.ObterFluxoCaixa;

public class ObterFluxoCaixaQueryHandler : IRequestHandler<ObterFluxoCaixaQuery, FluxoCaixaDto>
{
    private readonly IAppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public ObterFluxoCaixaQueryHandler(IAppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<FluxoCaixaDto> Handle(ObterFluxoCaixaQuery request, CancellationToken cancellationToken)
    {
        _ = _tenantProvider.ClinicaId
            ?? throw new UnauthorizedAccessException("Tenant não identificado.");

        var lancamentos = await _context.LancamentosFinanceiros
            .AsNoTracking()
            .Where(l => l.Competencia == request.Competencia && l.Status != StatusLancamento.Cancelado)
            .ToListAsync(cancellationToken);

        var totalReceitasPrevisto = lancamentos
            .Where(l => l.Tipo == TipoLancamento.Receita)
            .Sum(l => l.Valor);

        var totalReceitasConfirmado = lancamentos
            .Where(l => l.Tipo == TipoLancamento.Receita && l.Status == StatusLancamento.Confirmado)
            .Sum(l => l.Valor);

        var totalDespesasPrevisto = lancamentos
            .Where(l => l.Tipo == TipoLancamento.Despesa)
            .Sum(l => l.Valor);

        var totalDespesasConfirmado = lancamentos
            .Where(l => l.Tipo == TipoLancamento.Despesa && l.Status == StatusLancamento.Confirmado)
            .Sum(l => l.Valor);

        var dias = lancamentos
            .GroupBy(l => l.DataVencimento)
            .OrderBy(g => g.Key)
            .Select(g => new FluxoCaixaDiaDto(
                g.Key,
                g.Where(l => l.Tipo == TipoLancamento.Receita).Sum(l => l.Valor),
                g.Where(l => l.Tipo == TipoLancamento.Receita && l.Status == StatusLancamento.Confirmado).Sum(l => l.Valor),
                g.Where(l => l.Tipo == TipoLancamento.Despesa).Sum(l => l.Valor),
                g.Where(l => l.Tipo == TipoLancamento.Despesa && l.Status == StatusLancamento.Confirmado).Sum(l => l.Valor)
            )).ToList();

        return new FluxoCaixaDto(
            request.Competencia,
            totalReceitasPrevisto,
            totalReceitasConfirmado,
            totalDespesasPrevisto,
            totalDespesasConfirmado,
            SaldoPrevisto: totalReceitasPrevisto - totalDespesasPrevisto,
            SaldoRealizado: totalReceitasConfirmado - totalDespesasConfirmado,
            dias);
    }
}
