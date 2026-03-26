using MediatR;
using PsicoFinance.Application.Features.Pacientes.DTOs;

namespace PsicoFinance.Application.Features.Pacientes.Commands.AtualizarPaciente;

public record AtualizarPacienteCommand(
    Guid Id,
    string Nome,
    string? Cpf,
    string? Email,
    string? Telefone,
    DateOnly? DataNascimento,
    string? ResponsavelNome,
    string? ResponsavelTelefone,
    string? Observacoes) : IRequest<PacienteDto>;
