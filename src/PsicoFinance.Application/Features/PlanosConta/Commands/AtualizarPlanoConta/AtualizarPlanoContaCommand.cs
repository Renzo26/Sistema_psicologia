using MediatR;
using PsicoFinance.Application.Features.PlanosConta.DTOs;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.PlanosConta.Commands.AtualizarPlanoConta;

public record AtualizarPlanoContaCommand(
    Guid Id,
    string Nome,
    TipoPlanoConta Tipo,
    string? Descricao,
    bool Ativo) : IRequest<PlanoContaDto>;
