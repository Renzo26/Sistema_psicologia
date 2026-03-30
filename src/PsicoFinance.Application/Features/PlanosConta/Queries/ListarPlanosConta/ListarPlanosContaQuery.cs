using MediatR;
using PsicoFinance.Application.Features.PlanosConta.DTOs;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.PlanosConta.Queries.ListarPlanosConta;

public record ListarPlanosContaQuery(
    TipoPlanoConta? Tipo = null,
    bool? Ativo = null,
    string? Busca = null) : IRequest<List<PlanoContaDto>>;
