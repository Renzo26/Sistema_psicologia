using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Psicologos.DTOs;

namespace PsicoFinance.Application.Features.Psicologos.Queries.ListarPsicologos;

public class ListarPsicologosQueryHandler : IRequestHandler<ListarPsicologosQuery, List<PsicologoResumoDto>>
{
    private readonly IAppDbContext _context;

    public ListarPsicologosQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<PsicologoResumoDto>> Handle(ListarPsicologosQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Psicologos.AsNoTracking();

        if (request.ApenasAtivos == true)
            query = query.Where(p => p.Ativo);

        if (!string.IsNullOrWhiteSpace(request.Busca))
        {
            var busca = request.Busca.ToLower();
            query = query.Where(p =>
                p.Nome.ToLower().Contains(busca) ||
                p.Crp.ToLower().Contains(busca));
        }

        return await query
            .OrderBy(p => p.Nome)
            .Select(p => new PsicologoResumoDto(
                p.Id, p.Nome, p.Crp, p.Tipo,
                p.TipoRepasse, p.ValorRepasse, p.Ativo))
            .ToListAsync(cancellationToken);
    }
}
