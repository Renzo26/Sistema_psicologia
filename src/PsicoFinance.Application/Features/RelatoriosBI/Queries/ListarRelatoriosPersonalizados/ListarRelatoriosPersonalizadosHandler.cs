using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.RelatoriosBI.DTOs;

namespace PsicoFinance.Application.Features.RelatoriosBI.Queries.ListarRelatoriosPersonalizados;

public class ListarRelatoriosPersonalizadosHandler
    : IRequestHandler<ListarRelatoriosPersonalizadosQuery, List<RelatorioPersonalizadoDto>>
{
    private readonly IAppDbContext _context;

    public ListarRelatoriosPersonalizadosHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<RelatorioPersonalizadoDto>> Handle(
        ListarRelatoriosPersonalizadosQuery request,
        CancellationToken ct)
    {
        var query = _context.RelatoriosPersonalizados.AsNoTracking();

        if (request.Tipo.HasValue)
            query = query.Where(r => r.Tipo == request.Tipo.Value);

        if (request.ApenasFavorito == true)
            query = query.Where(r => r.Favorito);

        var lista = await query
            .OrderByDescending(r => r.Favorito)
            .ThenByDescending(r => r.AtualizadoEm)
            .ToListAsync(ct);

        return lista.Select(e => new RelatorioPersonalizadoDto
        {
            Id = e.Id,
            Nome = e.Nome,
            Descricao = e.Descricao,
            Tipo = e.Tipo,
            FiltrosJson = e.FiltrosJson,
            Agrupamento = e.Agrupamento,
            Ordenacao = e.Ordenacao,
            Favorito = e.Favorito,
            CriadoPorId = e.CriadoPorId,
            CriadoEm = e.CriadoEm,
            AtualizadoEm = e.AtualizadoEm
        }).ToList();
    }
}
