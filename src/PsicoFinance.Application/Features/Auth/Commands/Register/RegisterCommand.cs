using MediatR;
using PsicoFinance.Application.Features.Auth.DTOs;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string Nome,
    string Email,
    string Senha,
    RoleUsuario Role,
    Guid ClinicaId,
    Guid? PsicologoId
) : IRequest<UsuarioDto>;
