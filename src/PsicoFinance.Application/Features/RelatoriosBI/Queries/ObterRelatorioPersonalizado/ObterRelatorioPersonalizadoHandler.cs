using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.RelatoriosBI.DTOs;

namespace PsicoFinance.Application.Features.RelatoriosBI.Queries.ObterRelatorioPersonalizado;

public class ObterRelatorioPersonalizadoHandler
    : IRequestHandler<ObterRelatorioPersonalizadoQuery, RelatorioPersonalizadoDto>
{
    private readonly IAppDbContext _context;

    public ObterRelatorioPersonalizadoHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<RelatorioPersonalizadoDto> Handle(
        ObterRelatorioPersonalizadoQuery request,
        CancellationToken ct)
    {
        var entidade = await _context.RelatoriosPersonalizados
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.Id, ct)
            ?? throw new KeyNotFoundException("Relatório não encontrado.");

        return new RelatorioPersonalizadoDto
        {
            Id = entidade.Id,
            Nome = entidade.Nome,
            Descricao = entidade.Descricao,
            Tipo = entidade.Tipo,
            FiltrosJson = entidade.FiltrosJson,
            Agrupamento = entidade.Agrupamento,
            Ordenacao = entidade.Ordenacao,
            Favorito = entidade.Favorito,
            CriadoPorId = entidade.CriadoPorId,
            CriadoEm = entidade.CriadoEm,
            AtualizadoEm = entidade.AtualizadoEm
        };
    }
}
