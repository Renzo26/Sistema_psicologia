using MediatR;
using PsicoFinance.Application.Features.PlanosConta.DTOs;

namespace PsicoFinance.Application.Features.PlanosConta.Queries.ObterPlanoConta;

public record ObterPlanoContaQuery(Guid Id) : IRequest<PlanoContaDto>;
