using MediatR;

namespace PsicoFinance.Application.Features.Auth.Commands.TrocarSenha;

public record TrocarSenhaCommand(
    Guid UsuarioId,
    string SenhaAtual,
    string NovaSenha
) : IRequest<Unit>;
