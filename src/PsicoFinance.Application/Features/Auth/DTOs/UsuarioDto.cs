using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Auth.DTOs;

public record UsuarioDto(
    Guid Id,
    string Nome,
    string Email,
    RoleUsuario Role,
    Guid ClinicaId,
    bool Ativo,
    DateTimeOffset? UltimoAcessoEm
);
