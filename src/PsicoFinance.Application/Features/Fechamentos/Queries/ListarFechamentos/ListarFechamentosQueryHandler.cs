using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;

namespace PsicoFinance.Application.Features.Fechamentos.Queries.ListarFechamentos;

public class ListarFechamentosQueryHandler
    : IRequestHandler<ListarFechamentosQuery, List<FechamentoResumoDto>>
{
    private readonly IAppDbContext _context;

    public ListarFechamentosQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<FechamentoResumoDto>> Handle(
        ListarFechamentosQuery request, CancellationToken cancellationToken)
    {
        var query = _context.FechamentosMensais.AsNoTracking().AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(f => f.Status == request.Status.Value);

        return await query
            .OrderByDescending(f => f.MesReferencia)
            .Select(f => new FechamentoResumoDto(
                f.Id, f.MesReferencia, f.Status,
                f.TotalReceitas, f.TotalDespesas, f.Saldo,
                f.TotalSessoesRealizadas, f.FechadoEm))
            .ToListAsync(cancellationToken);
    }
}
