using MediatR;

namespace PsicoFinance.Application.Features.Psicologos.Commands.InativarPsicologo;

public record InativarPsicologoCommand(Guid Id) : IRequest;
