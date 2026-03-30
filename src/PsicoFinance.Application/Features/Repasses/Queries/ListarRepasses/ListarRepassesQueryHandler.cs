using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Repasses.Commands.GerarRepasseMensal;
using PsicoFinance.Application.Features.Repasses.DTOs;

namespace PsicoFinance.Application.Features.Repasses.Queries.ListarRepasses;

public class ListarRepassesQueryHandler : IRequestHandler<ListarRepassesQuery, List<RepasseDto>>
{
    private readonly IAppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public ListarRepassesQueryHandler(IAppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<List<RepasseDto>> Handle(ListarRepassesQuery request, CancellationToken cancellationToken)
    {
        _ = _tenantProvider.ClinicaId
            ?? throw new UnauthorizedAccessException("Tenant não identificado.");

        var query = _context.Repasses
            .AsNoTracking()
            .Include(r => r.Psicologo)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.MesReferencia))
            query = query.Where(r => r.MesReferencia == request.MesReferencia);

        if (request.PsicologoId.HasValue)
            query = query.Where(r => r.PsicologoId == request.PsicologoId.Value);

        if (request.Status.HasValue)
            query = query.Where(r => r.Status == request.Status.Value);

        var repasses = await query
            .OrderByDescending(r => r.MesReferencia)
            .ThenBy(r => r.Psicologo.Nome)
            .ToListAsync(cancellationToken);

        return repasses.Select(r => GerarRepasseMensalCommandHandler.MapToDto(r, r.Psicologo.Nome)).ToList();
    }
}
