using MediatR;

namespace PsicoFinance.Application.Features.Contratos.Commands.EncerrarContrato;

public record EncerrarContratoCommand(Guid Id, string? MotivoEncerramento) : IRequest;
