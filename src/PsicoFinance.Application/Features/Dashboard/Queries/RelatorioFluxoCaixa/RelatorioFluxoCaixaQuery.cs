using MediatR;
using PsicoFinance.Application.Features.Dashboard.DTOs;

namespace PsicoFinance.Application.Features.Dashboard.Queries.RelatorioFluxoCaixa;

/// <summary>
/// Retorna fluxo de caixa mensal para um intervalo de competências.
/// Ex: competenciaInicio="2025-01", competenciaFim="2025-12"
/// </summary>
public record RelatorioFluxoCaixaQuery(
    string CompetenciaInicio,
    string CompetenciaFim) : IRequest<List<RelatorioFluxoCaixaMensalDto>>;
