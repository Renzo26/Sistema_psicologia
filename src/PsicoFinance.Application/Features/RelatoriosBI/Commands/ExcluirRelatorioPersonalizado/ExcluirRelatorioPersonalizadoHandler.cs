using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;

namespace PsicoFinance.Application.Features.RelatoriosBI.Commands.ExcluirRelatorioPersonalizado;

public class ExcluirRelatorioPersonalizadoHandler : IRequestHandler<ExcluirRelatorioPersonalizadoCommand, Unit>
{
    private readonly IAppDbContext _context;

    public ExcluirRelatorioPersonalizadoHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(ExcluirRelatorioPersonalizadoCommand request, CancellationToken ct)
    {
        var entidade = await _context.RelatoriosPersonalizados
            .FirstOrDefaultAsync(r => r.Id == request.Id, ct)
            ?? throw new KeyNotFoundException("Relatório não encontrado.");

        _context.RelatoriosPersonalizados.Remove(entidade);
        await _context.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
