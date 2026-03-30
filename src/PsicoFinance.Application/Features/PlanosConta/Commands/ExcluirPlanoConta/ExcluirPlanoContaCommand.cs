using MediatR;

namespace PsicoFinance.Application.Features.PlanosConta.Commands.ExcluirPlanoConta;

public record ExcluirPlanoContaCommand(Guid Id) : IRequest;
