using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;

namespace PsicoFinance.Application.Features.Psicologos.Commands.InativarPsicologo;

public class InativarPsicologoCommandHandler : IRequestHandler<InativarPsicologoCommand>
{
    private readonly IAppDbContext _context;

    public InativarPsicologoCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(InativarPsicologoCommand request, CancellationToken cancellationToken)
    {
        var psicologo = await _context.Psicologos
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Psicólogo não encontrado.");

        psicologo.Ativo = false;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
