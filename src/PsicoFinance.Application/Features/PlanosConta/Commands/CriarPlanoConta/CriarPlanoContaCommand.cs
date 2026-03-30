using MediatR;
using PsicoFinance.Application.Features.PlanosConta.DTOs;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.PlanosConta.Commands.CriarPlanoConta;

public record CriarPlanoContaCommand(
    string Nome,
    TipoPlanoConta Tipo,
    string? Descricao) : IRequest<PlanoContaDto>;
