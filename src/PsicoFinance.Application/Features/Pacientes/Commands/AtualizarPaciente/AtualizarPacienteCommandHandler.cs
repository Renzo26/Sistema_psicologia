using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Pacientes.DTOs;

namespace PsicoFinance.Application.Features.Pacientes.Commands.AtualizarPaciente;

public class AtualizarPacienteCommandHandler : IRequestHandler<AtualizarPacienteCommand, PacienteDto>
{
    private readonly IAppDbContext _context;

    public AtualizarPacienteCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<PacienteDto> Handle(AtualizarPacienteCommand request, CancellationToken cancellationToken)
    {
        var paciente = await _context.Pacientes
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Paciente não encontrado.");

        // Verificar CPF duplicado (excluindo o próprio)
        if (!string.IsNullOrWhiteSpace(request.Cpf))
        {
            var cpfExiste = await _context.Pacientes
                .AnyAsync(p => p.Cpf == request.Cpf && p.Id != request.Id, cancellationToken);

            if (cpfExiste)
                throw new InvalidOperationException("Já existe outro paciente com este CPF na clínica.");
        }

        paciente.Nome = request.Nome;
        paciente.Cpf = request.Cpf;
        paciente.Email = request.Email;
        paciente.Telefone = request.Telefone;
        paciente.DataNascimento = request.DataNascimento;
        paciente.ResponsavelNome = request.ResponsavelNome;
        paciente.ResponsavelTelefone = request.ResponsavelTelefone;
        paciente.Observacoes = request.Observacoes;

        await _context.SaveChangesAsync(cancellationToken);

        return new PacienteDto(
            paciente.Id, paciente.Nome, paciente.Cpf, paciente.Email,
            paciente.Telefone, paciente.DataNascimento,
            paciente.ResponsavelNome, paciente.ResponsavelTelefone,
            paciente.Observacoes, paciente.Ativo, paciente.CriadoEm);
    }
}
