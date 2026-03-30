using MediatR;
using PsicoFinance.Application.Features.Lancamentos.DTOs;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Lancamentos.Queries.ListarLancamentos;

public record ListarLancamentosQuery(
    string? Competencia = null,
    TipoLancamento? Tipo = null,
    StatusLancamento? Status = null,
    Guid? PlanoContaId = null,
    DateOnly? DataInicio = null,
    DateOnly? DataFim = null) : IRequest<List<LancamentoDto>>;
