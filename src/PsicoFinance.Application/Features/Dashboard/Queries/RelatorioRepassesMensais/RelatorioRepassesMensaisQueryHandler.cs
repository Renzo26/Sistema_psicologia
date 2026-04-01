using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Dashboard.DTOs;

namespace PsicoFinance.Application.Features.Dashboard.Queries.RelatorioRepassesMensais;

public class RelatorioRepassesMensaisQueryHandler
    : IRequestHandler<RelatorioRepassesMensaisQuery, RelatorioRepassesMensaisDto>
{
    private readonly IAppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public RelatorioRepassesMensaisQueryHandler(IAppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<RelatorioRepassesMensaisDto> Handle(
        RelatorioRepassesMensaisQuery request, CancellationToken cancellationToken)
    {
        _ = _tenantProvider.ClinicaId
            ?? throw new UnauthorizedAccessException("Tenant não identificado.");

        var query = _context.Repasses
            .AsNoTracking()
            .Include(r => r.Psicologo)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.CompetenciaInicio))
            query = query.Where(r => string.Compare(r.MesReferencia, request.CompetenciaInicio) >= 0);

        if (!string.IsNullOrWhiteSpace(request.CompetenciaFim))
            query = query.Where(r => string.Compare(r.MesReferencia, request.CompetenciaFim) <= 0);

        var repasses = await query
            .OrderBy(r => r.MesReferencia)
            .ThenBy(r => r.Psicologo!.Nome)
            .ToListAsync(cancellationToken);

        var itens = repasses.Select(r => new RepasseMensalItemDto(
            r.MesReferencia,
            r.PsicologoId,
            r.Psicologo?.Nome ?? "—",
            r.ValorRepasse,
            r.Status.ToString())).ToList();

        return new RelatorioRepassesMensaisDto(itens);
    }
}
