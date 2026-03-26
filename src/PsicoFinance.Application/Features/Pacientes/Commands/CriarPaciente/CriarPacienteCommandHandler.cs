using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Pacientes.DTOs;
using PsicoFinance.Domain.Entities;

namespace PsicoFinance.Application.Features.Pacientes.Commands.CriarPaciente;

public class CriarPacienteCommandHandler : IRequestHandler<CriarPacienteCommand, PacienteDto>
{
    private readonly IAppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public CriarPacienteCommandHandler(IAppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<PacienteDto> Handle(CriarPacienteCommand request, CancellationToken cancellationToken)
    {
        var clinicaId = _tenantProvider.ClinicaId
            ?? throw new UnauthorizedAccessException("Tenant não identificado.");

        // Verificar CPF duplicado na mesma clínica (se informado)
        if (!string.IsNullOrWhiteSpace(request.Cpf))
        {
            var cpfExiste = await _context.Pacientes
                .AnyAsync(p => p.Cpf == request.Cpf, cancellationToken);

            if (cpfExiste)
                throw new InvalidOperationException("Já existe um paciente com este CPF na clínica.");
        }

        var paciente = new Paciente
        {
            Id = Guid.NewGuid(),
            ClinicaId = clinicaId,
            Nome = request.Nome,
            Cpf = request.Cpf,
            Email = request.Email,
            Telefone = request.Telefone,
            DataNascimento = request.DataNascimento,
            ResponsavelNome = request.ResponsavelNome,
            ResponsavelTelefone = request.ResponsavelTelefone,
            Observacoes = request.Observacoes,
            Ativo = true
        };

        _context.Pacientes.Add(paciente);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(paciente);
    }

    private static PacienteDto MapToDto(Paciente p) => new(
        p.Id, p.Nome, p.Cpf, p.Email, p.Telefone,
        p.DataNascimento, p.ResponsavelNome, p.ResponsavelTelefone,
        p.Observacoes, p.Ativo, p.CriadoEm);
}
