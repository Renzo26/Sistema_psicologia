using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;

namespace PsicoFinance.Application.Features.Pacientes.Commands.InativarPaciente;

public class InativarPacienteCommandHandler : IRequestHandler<InativarPacienteCommand>
{
    private readonly IAppDbContext _context;

    public InativarPacienteCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(InativarPacienteCommand request, CancellationToken cancellationToken)
    {
        var paciente = await _context.Pacientes
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Paciente não encontrado.");

        paciente.Ativo = false;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
