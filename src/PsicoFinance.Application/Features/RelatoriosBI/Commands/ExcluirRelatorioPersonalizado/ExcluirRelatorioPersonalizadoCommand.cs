using MediatR;

namespace PsicoFinance.Application.Features.RelatoriosBI.Commands.ExcluirRelatorioPersonalizado;

public record ExcluirRelatorioPersonalizadoCommand(Guid Id) : IRequest<Unit>;
