using MediatR;
using PsicoFinance.Application.Features.Auth.DTOs;

namespace PsicoFinance.Application.Features.Auth.Commands.Login;

public record LoginCommand(
    string Email,
    string Senha,
    string? IpOrigem,
    string? UserAgent
) : IRequest<AuthResponseDto>;
