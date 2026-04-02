using MediatR;
using PsicoFinance.Application.Features.RelatoriosBI.DTOs;

namespace PsicoFinance.Application.Features.RelatoriosBI.Commands.AtualizarRelatorioPersonalizado;

public record AtualizarRelatorioPersonalizadoCommand(
    Guid Id,
    string Nome,
    string? Descricao,
    RelatorioFiltrosDto Filtros,
    string? Agrupamento,
    string? Ordenacao
) : IRequest<RelatorioPersonalizadoDto>;
