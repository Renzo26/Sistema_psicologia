using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Auth.DTOs;

public record AuthResponseDto(
    Guid UsuarioId,
    string Nome,
    string Email,
    RoleUsuario Role,
    Guid ClinicaId,
    string AccessToken
);
