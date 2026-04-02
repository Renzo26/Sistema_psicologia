using MediatR;
using PsicoFinance.Application.Features.RelatoriosBI.DTOs;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.RelatoriosBI.Queries.ExecutarRelatorio;

public record ExecutarRelatorioQuery(
    Guid? Id,
    TipoRelatorio Tipo,
    RelatorioFiltrosDto Filtros,
    string? Agrupamento,
    string? Ordenacao
) : IRequest<RelatorioResultadoDto>;
