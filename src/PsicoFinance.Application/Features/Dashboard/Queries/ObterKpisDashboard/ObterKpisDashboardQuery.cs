using MediatR;
using PsicoFinance.Application.Features.Dashboard.DTOs;

namespace PsicoFinance.Application.Features.Dashboard.Queries.ObterKpisDashboard;

/// <summary>
/// Retorna todos os KPIs para o período (competencia = "YYYY-MM").
/// Se não informado, usa o mês atual.
/// </summary>
public record ObterKpisDashboardQuery(string? Competencia) : IRequest<KpisDashboardDto>;
