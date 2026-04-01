using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Dashboard.DTOs;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Dashboard.Queries.RelatorioInadimplencia;

public class RelatorioInadimplenciaQueryHandler
    : IRequestHandler<RelatorioInadimplenciaQuery, RelatorioInadimplenciaDto>
{
    private readonly IAppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public RelatorioInadimplenciaQueryHandler(IAppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<RelatorioInadimplenciaDto> Handle(
        RelatorioInadimplenciaQuery request, CancellationToken cancellationToken)
    {
        _ = _tenantProvider.ClinicaId
            ?? throw new UnauthorizedAccessException("Tenant não identificado.");

        var dataBase = request.DataBase ?? DateOnly.FromDateTime(DateTime.UtcNow);

        // Lançamentos de receita, Previsto, vencidos
        var vencidos = await _context.LancamentosFinanceiros
            .AsNoTracking()
            .Include(l => l.Sessao)
                .ThenInclude(s => s!.Paciente)
            .Where(l => l.Tipo == TipoLancamento.Receita
                     && l.Status == StatusLancamento.Previsto
                     && l.DataVencimento < dataBase)
            .OrderBy(l => l.DataVencimento)
            .ToListAsync(cancellationToken);

        var itens = vencidos.Select(l =>
        {
            var pacienteNome = l.Sessao?.Paciente?.Nome ?? "—";
            var pacienteId = l.Sessao?.PacienteId ?? Guid.Empty;
            var dias = dataBase.DayNumber - l.DataVencimento.DayNumber;
            return new InadimplenciaItemDto(
                pacienteId,
                pacienteNome,
                l.Id,
                l.Descricao,
                l.Valor,
                l.DataVencimento,
                dias);
        }).ToList();

        return new RelatorioInadimplenciaDto(
            dataBase,
            itens.Select(i => i.PacienteId).Distinct().Count(),
            itens.Count,
            itens.Sum(i => i.Valor),
            itens);
    }
}
