using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.RelatoriosBI.DTOs;

namespace PsicoFinance.Application.Features.RelatoriosBI.Commands.AtualizarRelatorioPersonalizado;

public class AtualizarRelatorioPersonalizadoHandler : IRequestHandler<AtualizarRelatorioPersonalizadoCommand, RelatorioPersonalizadoDto>
{
    private readonly IAppDbContext _context;

    public AtualizarRelatorioPersonalizadoHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<RelatorioPersonalizadoDto> Handle(
        AtualizarRelatorioPersonalizadoCommand request,
        CancellationToken ct)
    {
        var entidade = await _context.RelatoriosPersonalizados
            .FirstOrDefaultAsync(r => r.Id == request.Id, ct)
            ?? throw new KeyNotFoundException("Relatório não encontrado.");

        entidade.Nome = request.Nome;
        entidade.Descricao = request.Descricao;
        entidade.FiltrosJson = JsonSerializer.Serialize(request.Filtros);
        entidade.Agrupamento = request.Agrupamento;
        entidade.Ordenacao = request.Ordenacao;

        await _context.SaveChangesAsync(ct);

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
