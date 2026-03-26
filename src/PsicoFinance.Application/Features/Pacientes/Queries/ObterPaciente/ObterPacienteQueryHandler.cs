using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Pacientes.DTOs;

namespace PsicoFinance.Application.Features.Pacientes.Queries.ObterPaciente;

public class ObterPacienteQueryHandler : IRequestHandler<ObterPacienteQuery, PacienteDto>
{
    private readonly IAppDbContext _context;

    public ObterPacienteQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<PacienteDto> Handle(ObterPacienteQuery request, CancellationToken cancellationToken)
    {
        var p = await _context.Pacientes
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Paciente não encontrado.");

        return new PacienteDto(
            p.Id, p.Nome, p.Cpf, p.Email, p.Telefone,
            p.DataNascimento, p.ResponsavelNome, p.ResponsavelTelefone,
            p.Observacoes, p.Ativo, p.CriadoEm);
    }
}
