using MediatR;
using PsicoFinance.Application.Features.RelatoriosBI.DTOs;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.RelatoriosBI.Commands.CriarRelatorioPersonalizado;

public record CriarRelatorioPersonalizadoCommand(
    string Nome,
    string? Descricao,
    TipoRelatorio Tipo,
    RelatorioFiltrosDto Filtros,
    string? Agrupamento,
    string? Ordenacao
) : IRequest<RelatorioPersonalizadoDto>;
