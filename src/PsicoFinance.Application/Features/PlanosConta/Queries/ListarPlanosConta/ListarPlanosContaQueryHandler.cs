using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.PlanosConta.Commands.CriarPlanoConta;
using PsicoFinance.Application.Features.PlanosConta.DTOs;

namespace PsicoFinance.Application.Features.PlanosConta.Queries.ListarPlanosConta;

public class ListarPlanosContaQueryHandler : IRequestHandler<ListarPlanosContaQuery, List<PlanoContaDto>>
{
    private readonly IAppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public ListarPlanosContaQueryHandler(IAppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<List<PlanoContaDto>> Handle(ListarPlanosContaQuery request, CancellationToken cancellationToken)
    {
        _ = _tenantProvider.ClinicaId
            ?? throw new UnauthorizedAccessException("Tenant não identificado.");

        var query = _context.PlanosConta.AsNoTracking().AsQueryable();

        if (request.Tipo.HasValue)
            query = query.Where(p => p.Tipo == request.Tipo.Value);

        if (request.Ativo.HasValue)
            query = query.Where(p => p.Ativo == request.Ativo.Value);

        if (!string.IsNullOrWhiteSpace(request.Busca))
            query = query.Where(p => p.Nome.Contains(request.Busca));

        var planos = await query
            .OrderBy(p => p.Tipo)
            .ThenBy(p => p.Nome)
            .ToListAsync(cancellationToken);

        return planos.Select(CriarPlanoContaCommandHandler.MapToDto).ToList();
    }
}
