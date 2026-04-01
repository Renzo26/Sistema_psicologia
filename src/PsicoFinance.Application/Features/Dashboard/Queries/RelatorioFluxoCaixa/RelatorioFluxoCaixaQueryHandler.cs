using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Dashboard.DTOs;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Dashboard.Queries.RelatorioFluxoCaixa;

public class RelatorioFluxoCaixaQueryHandler
    : IRequestHandler<RelatorioFluxoCaixaQuery, List<RelatorioFluxoCaixaMensalDto>>
{
    private readonly IAppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public RelatorioFluxoCaixaQueryHandler(IAppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<List<RelatorioFluxoCaixaMensalDto>> Handle(
        RelatorioFluxoCaixaQuery request, CancellationToken cancellationToken)
    {
        _ = _tenantProvider.ClinicaId
            ?? throw new UnauthorizedAccessException("Tenant não identificado.");

        // Gera lista de competências no intervalo
        var competencias = GerarIntervalo(request.CompetenciaInicio, request.CompetenciaFim);

        var lancamentos = await _context.LancamentosFinanceiros
            .AsNoTracking()
            .Where(l => competencias.Contains(l.Competencia) && l.Status != StatusLancamento.Cancelado)
            .ToListAsync(cancellationToken);

        return competencias.Select(comp =>
        {
            var lcomp = lancamentos.Where(l => l.Competencia == comp).ToList();
            var recPrev = lcomp.Where(l => l.Tipo == TipoLancamento.Receita).Sum(l => l.Valor);
            var recConf = lcomp.Where(l => l.Tipo == TipoLancamento.Receita && l.Status == StatusLancamento.Confirmado).Sum(l => l.Valor);
            var despPrev = lcomp.Where(l => l.Tipo == TipoLancamento.Despesa).Sum(l => l.Valor);
            var despConf = lcomp.Where(l => l.Tipo == TipoLancamento.Despesa && l.Status == StatusLancamento.Confirmado).Sum(l => l.Valor);

            return new RelatorioFluxoCaixaMensalDto(comp, recPrev, recConf, despPrev, despConf,
                SaldoPrevisto: recPrev - despPrev,
                SaldoRealizado: recConf - despConf);
        }).ToList();
    }

    private static List<string> GerarIntervalo(string inicio, string fim)
    {
        var resultado = new List<string>();
        var partsI = inicio.Split('-');
        var partsF = fim.Split('-');
        var atual = new DateOnly(int.Parse(partsI[0]), int.Parse(partsI[1]), 1);
        var limite = new DateOnly(int.Parse(partsF[0]), int.Parse(partsF[1]), 1);

        while (atual <= limite)
        {
            resultado.Add($"{atual.Year:D4}-{atual.Month:D2}");
            atual = atual.AddMonths(1);
        }
        return resultado;
    }
}
