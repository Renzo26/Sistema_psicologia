using MediatR;

namespace PsicoFinance.Application.Features.Pacientes.Commands.InativarPaciente;

public record InativarPacienteCommand(Guid Id) : IRequest;
