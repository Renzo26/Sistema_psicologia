using MediatR;

namespace PsicoFinance.Application.Features.Sessoes.Commands.MarcarPresenca;

public record MarcarPresencaCommand(Guid Id) : IRequest;
