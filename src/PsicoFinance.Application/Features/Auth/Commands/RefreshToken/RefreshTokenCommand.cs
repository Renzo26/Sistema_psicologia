using MediatR;
using PsicoFinance.Application.Features.Auth.DTOs;

namespace PsicoFinance.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(
    string RefreshToken,
    string? IpOrigem,
    string? UserAgent
) : IRequest<AuthResponseDto>;
