using MediatR;
using PsicoFinance.Application.Features.Onboarding.DTOs;

namespace PsicoFinance.Application.Features.Onboarding.Commands;

public record OnboardingCommand(
    string NomeClinica,
    string? Cnpj,
    string EmailClinica,
    string? Telefone,
    string NomeAdmin,
    string EmailAdmin,
    string SenhaAdmin,
    string? IpOrigem,
    string? UserAgent) : IRequest<OnboardingResponseDto>;
