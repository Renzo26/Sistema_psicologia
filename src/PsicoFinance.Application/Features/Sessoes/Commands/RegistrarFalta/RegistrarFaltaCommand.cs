using MediatR;

namespace PsicoFinance.Application.Features.Sessoes.Commands.RegistrarFalta;

public record RegistrarFaltaCommand(Guid Id, bool Justificada, string? Motivo) : IRequest;
