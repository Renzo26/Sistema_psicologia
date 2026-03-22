using MediatR;

namespace PsicoFinance.Application.Features.Auth.Commands.RecuperarSenha;

public record SolicitarRecuperacaoSenhaCommand(string Email) : IRequest<Unit>;

public record RedefinirSenhaCommand(
    string Token,
    string NovaSenha
) : IRequest<Unit>;
