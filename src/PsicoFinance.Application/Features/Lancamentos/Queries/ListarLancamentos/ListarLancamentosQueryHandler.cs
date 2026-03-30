using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Lancamentos.Commands.CriarLancamento;
using PsicoFinance.Application.Features.Lancamentos.DTOs;

namespace PsicoFinance.Application.Features.Lancamentos.Queries.ListarLancamentos;

public class ListarLancamentosQueryHandler : IRequestHandler<ListarLancamentosQuery, List<LancamentoDto>>
{
    private readonly IAppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public ListarLancamentosQueryHandler(IAppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<List<LancamentoDto>> Handle(ListarLancamentosQuery request, CancellationToken cancellationToken)
    {
        _ = _tenantProvider.ClinicaId
            ?? throw new UnauthorizedAccessException("Tenant não identificado.");

        var query = _context.LancamentosFinanceiros
            .AsNoTracking()
            .Include(l => l.PlanoConta)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Competencia))
            query = query.Where(l => l.Competencia == request.Competencia);

        if (request.Tipo.HasValue)
            query = query.Where(l => l.Tipo == request.Tipo.Value);

        if (request.Status.HasValue)
            query = query.Where(l => l.Status == request.Status.Value);

        if (request.PlanoContaId.HasValue)
            query = query.Where(l => l.PlanoContaId == request.PlanoContaId.Value);

        if (request.DataInicio.HasValue)
            query = query.Where(l => l.DataVencimento >= request.DataInicio.Value);

        if (request.DataFim.HasValue)
            query = query.Where(l => l.DataVencimento <= request.DataFim.Value);

        var lancamentos = await query
            .OrderBy(l => l.DataVencimento)
            .ThenBy(l => l.Tipo)
            .ToListAsync(cancellationToken);

        return lancamentos.Select(l =>
            CriarLancamentoCommandHandler.MapToDto(l, l.PlanoConta?.Nome ?? string.Empty))
            .ToList();
    }
}
