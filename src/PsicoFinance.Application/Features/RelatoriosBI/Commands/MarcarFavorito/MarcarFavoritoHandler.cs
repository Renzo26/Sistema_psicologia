using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;

namespace PsicoFinance.Application.Features.RelatoriosBI.Commands.MarcarFavorito;

public class MarcarFavoritoHandler : IRequestHandler<MarcarFavoritoCommand, Unit>
{
    private readonly IAppDbContext _context;

    public MarcarFavoritoHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(MarcarFavoritoCommand request, CancellationToken ct)
    {
        var entidade = await _context.RelatoriosPersonalizados
            .FirstOrDefaultAsync(r => r.Id == request.Id, ct)
            ?? throw new KeyNotFoundException("Relatório não encontrado.");

        entidade.Favorito = request.Favorito;
        await _context.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
