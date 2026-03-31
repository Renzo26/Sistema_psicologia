using MediatR;
using PsicoFinance.Application.Features.Fechamentos.DTOs;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Fechamentos.Queries.ListarFechamentos;

public record ListarFechamentosQuery(StatusFechamento? Status) : IRequest<List<FechamentoResumoDto>>;

public record FechamentoResumoDto(
    Guid Id,
    string MesReferencia,
    StatusFechamento Status,
    decimal TotalReceitas,
    decimal TotalDespesas,
    decimal Saldo,
    int TotalSessoesRealizadas,
    DateTimeOffset? FechadoEm);
