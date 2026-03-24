namespace PsicoFinance.Application.Features.Onboarding.DTOs;

public record OnboardingResponseDto(
    Guid ClinicaId,
    Guid UsuarioId,
    string NomeClinica,
    string NomeUsuario,
    string Email,
    string AccessToken);
