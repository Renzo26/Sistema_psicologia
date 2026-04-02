using MediatR;
using PsicoFinance.Application.Features.RelatoriosBI.DTOs;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.RelatoriosBI.Queries.ListarRelatoriosPersonalizados;

public record ListarRelatoriosPersonalizadosQuery(
    TipoRelatorio? Tipo,
    bool? ApenasFavorito
) : IRequest<List<RelatorioPersonalizadoDto>>;
