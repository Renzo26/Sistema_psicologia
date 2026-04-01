using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Dashboard.DTOs;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Dashboard.Queries.ObterKpisDashboard;

public class ObterKpisDashboardQueryHandler : IRequestHandler<ObterKpisDashboardQuery, KpisDashboardDto>
{
    private readonly IAppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public ObterKpisDashboardQueryHandler(IAppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<KpisDashboardDto> Handle(ObterKpisDashboardQuery request, CancellationToken cancellationToken)
    {
        _ = _tenantProvider.ClinicaId
            ?? throw new UnauthorizedAccessException("Tenant não identificado.");

        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var competencia = request.Competencia ?? $"{hoje.Year:D4}-{hoje.Month:D2}";

        // ── Lançamentos do período ────────────────────────────────
        var lancamentos = await _context.LancamentosFinanceiros
            .AsNoTracking()
            .Where(l => l.Competencia == competencia && l.Status != StatusLancamento.Cancelado)
            .ToListAsync(cancellationToken);

        var receitasPrevistas = lancamentos
            .Where(l => l.Tipo == TipoLancamento.Receita)
            .Sum(l => l.Valor);

        var receitasConfirmadas = lancamentos
            .Where(l => l.Tipo == TipoLancamento.Receita && l.Status == StatusLancamento.Confirmado)
            .Sum(l => l.Valor);

        var despesasPrevistas = lancamentos
            .Where(l => l.Tipo == TipoLancamento.Despesa)
            .Sum(l => l.Valor);

        var despesasConfirmadas = lancamentos
            .Where(l => l.Tipo == TipoLancamento.Despesa && l.Status == StatusLancamento.Confirmado)
            .Sum(l => l.Valor);

        // ── Sessões do período ────────────────────────────────────
        // Competência "YYYY-MM" → filtrar sessões pelo mês
        var ano = int.Parse(competencia[..4]);
        var mes = int.Parse(competencia[5..]);
        var inicioMes = new DateOnly(ano, mes, 1);
        var fimMes = inicioMes.AddMonths(1).AddDays(-1);

        var sessoes = await _context.Sessoes
            .AsNoTracking()
            .Include(s => s.Psicologo)
            .Include(s => s.Paciente)
            .Where(s => s.Data >= inicioMes && s.Data <= fimMes)
            .ToListAsync(cancellationToken);

        var totalAgendadas = sessoes.Count(s => s.Status != StatusSessao.Cancelada);
        var totalRealizadas = sessoes.Count(s => s.Status == StatusSessao.Realizada);
        var totalFaltas = sessoes.Count(s => s.Status == StatusSessao.Falta);
        var totalFaltasJust = sessoes.Count(s => s.Status == StatusSessao.FaltaJustificada);
        var totalCanceladas = sessoes.Count(s => s.Status == StatusSessao.Cancelada);

        var taxaAbsenteismo = totalAgendadas > 0
            ? Math.Round((decimal)(totalFaltas + totalFaltasJust) / totalAgendadas * 100, 2)
            : 0m;

        var taxaOcupacao = totalAgendadas > 0
            ? Math.Round((decimal)totalRealizadas / totalAgendadas * 100, 2)
            : 0m;

        // Ticket médio: receitas confirmadas / sessões realizadas
        var ticketMedio = totalRealizadas > 0
            ? Math.Round(receitasConfirmadas / totalRealizadas, 2)
            : 0m;

        // Taxa de inadimplência: previsto vencido / total receita prevista
        var inadimplentesValor = lancamentos
            .Where(l => l.Tipo == TipoLancamento.Receita
                     && l.Status == StatusLancamento.Previsto
                     && l.DataVencimento < hoje)
            .Sum(l => l.Valor);

        var taxaInadimplencia = receitasPrevistas > 0
            ? Math.Round(inadimplentesValor / receitasPrevistas * 100, 2)
            : 0m;

        // ── Ranking psicólogos ────────────────────────────────────
        var ranking = sessoes
            .Where(s => s.Status != StatusSessao.Cancelada)
            .GroupBy(s => new { s.PsicologoId, Nome = s.Psicologo?.Nome ?? "—" })
            .Select(g =>
            {
                var total = g.Count();
                var realizadas = g.Count(s => s.Status == StatusSessao.Realizada);
                var faltas = g.Count(s => s.Status is StatusSessao.Falta or StatusSessao.FaltaJustificada);
                var taxa = total > 0 ? Math.Round((decimal)faltas / total * 100, 2) : 0m;

                // Receita gerada = lançamentos confirmados vinculados às sessões do psicólogo
                var sessaoIds = g.Where(s => s.Status == StatusSessao.Realizada)
                                 .Select(s => s.Id).ToHashSet();
                var receita = lancamentos
                    .Where(l => l.SessaoId.HasValue
                             && sessaoIds.Contains(l.SessaoId.Value)
                             && l.Status == StatusLancamento.Confirmado)
                    .Sum(l => l.Valor);

                return new RankingPsicologoDto(
                    g.Key.PsicologoId, g.Key.Nome, total, realizadas, receita, taxa);
            })
            .OrderByDescending(r => r.SessoesRealizadas)
            .ThenByDescending(r => r.ReceitaGerada)
            .ToList();

        // ── Pacientes inadimplentes ───────────────────────────────
        var lancamentosVencidos = await _context.LancamentosFinanceiros
            .AsNoTracking()
            .Include(l => l.Sessao)
            .Where(l => l.Tipo == TipoLancamento.Receita
                     && l.Status == StatusLancamento.Previsto
                     && l.DataVencimento < hoje
                     && l.Competencia == competencia)
            .ToListAsync(cancellationToken);

        var pacientesInadimplentes = new List<InadimplentePacienteDto>();
        if (lancamentosVencidos.Count > 0)
        {
            var pacienteIds = lancamentosVencidos
                .Where(l => l.Sessao?.PacienteId != null)
                .Select(l => l.Sessao!.PacienteId)
                .Distinct()
                .ToList();

            var pacientes = await _context.Pacientes
                .AsNoTracking()
                .Where(p => pacienteIds.Contains(p.Id))
                .Select(p => new { p.Id, p.Nome })
                .ToListAsync(cancellationToken);

            pacientesInadimplentes = lancamentosVencidos
                .Where(l => l.Sessao?.PacienteId != null)
                .GroupBy(l => l.Sessao!.PacienteId)
                .Select(g =>
                {
                    var nome = pacientes.FirstOrDefault(p => p.Id == g.Key)?.Nome ?? "—";
                    return new InadimplentePacienteDto(
                        g.Key,
                        nome,
                        g.Count(),
                        g.Sum(l => l.Valor),
                        g.Min(l => l.DataVencimento));
                })
                .OrderBy(p => p.VencimentoMaisAntigo)
                .ToList();
        }

        return new KpisDashboardDto(
            competencia,
            receitasPrevistas,
            receitasConfirmadas,
            despesasPrevistas,
            despesasConfirmadas,
            SaldoPrevisto: receitasPrevistas - despesasPrevistas,
            SaldoRealizado: receitasConfirmadas - despesasConfirmadas,
            ticketMedio,
            taxaInadimplencia,
            totalAgendadas,
            totalRealizadas,
            totalFaltas,
            totalFaltasJust,
            totalCanceladas,
            taxaAbsenteismo,
            taxaOcupacao,
            ranking,
            pacientesInadimplentes);
    }
}
