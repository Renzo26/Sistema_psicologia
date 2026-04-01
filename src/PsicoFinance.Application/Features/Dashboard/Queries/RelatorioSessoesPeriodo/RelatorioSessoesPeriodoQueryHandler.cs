using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Dashboard.DTOs;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Dashboard.Queries.RelatorioSessoesPeriodo;

public class RelatorioSessoesPeriodoQueryHandler
    : IRequestHandler<RelatorioSessoesPeriodoQuery, RelatorioSessoesPeriodoDto>
{
    private readonly IAppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public RelatorioSessoesPeriodoQueryHandler(IAppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<RelatorioSessoesPeriodoDto> Handle(
        RelatorioSessoesPeriodoQuery request, CancellationToken cancellationToken)
    {
        _ = _tenantProvider.ClinicaId
            ?? throw new UnauthorizedAccessException("Tenant não identificado.");

        var sessoes = await _context.Sessoes
            .AsNoTracking()
            .Include(s => s.Psicologo)
            .Where(s => s.Data >= request.DataInicio && s.Data <= request.DataFim)
            .ToListAsync(cancellationToken);

        var totalAgendadas = sessoes.Count(s => s.Status != StatusSessao.Cancelada);
        var totalRealizadas = sessoes.Count(s => s.Status == StatusSessao.Realizada);
        var totalFaltas = sessoes.Count(s => s.Status is StatusSessao.Falta or StatusSessao.FaltaJustificada);
        var totalCanceladas = sessoes.Count(s => s.Status == StatusSessao.Cancelada);
        var taxaAbsenteismo = totalAgendadas > 0
            ? Math.Round((decimal)totalFaltas / totalAgendadas * 100, 2)
            : 0m;

        var porPsicologo = sessoes
            .GroupBy(s => new { s.PsicologoId, Nome = s.Psicologo?.Nome ?? "—" })
            .Select(g => new SessoesPorPsicologoDto(
                g.Key.PsicologoId,
                g.Key.Nome,
                g.Count(),
                g.Count(s => s.Status == StatusSessao.Realizada),
                g.Count(s => s.Status is StatusSessao.Falta or StatusSessao.FaltaJustificada),
                g.Count(s => s.Status == StatusSessao.Cancelada)))
            .OrderByDescending(p => p.Total)
            .ToList();

        return new RelatorioSessoesPeriodoDto(
            request.DataInicio,
            request.DataFim,
            totalAgendadas,
            totalRealizadas,
            totalFaltas,
            totalCanceladas,
            taxaAbsenteismo,
            porPsicologo);
    }
}
