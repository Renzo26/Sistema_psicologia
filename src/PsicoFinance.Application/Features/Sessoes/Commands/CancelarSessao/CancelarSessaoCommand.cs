using MediatR;

namespace PsicoFinance.Application.Features.Sessoes.Commands.CancelarSessao;

public record CancelarSessaoCommand(Guid Id, string? Motivo) : IRequest;
