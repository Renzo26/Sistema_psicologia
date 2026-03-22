using MediatR;

namespace PsicoFinance.Application.Features.Auth.Commands.Logout;

public record LogoutCommand(string RefreshToken) : IRequest<Unit>;
