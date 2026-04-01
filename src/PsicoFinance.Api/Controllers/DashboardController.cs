using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsicoFinance.Application.Features.Dashboard.Queries.ObterKpisDashboard;
using PsicoFinance.Application.Features.Dashboard.Queries.RelatorioFluxoCaixa;
using PsicoFinance.Application.Features.Dashboard.Queries.RelatorioInadimplencia;
using PsicoFinance.Application.Features.Dashboard.Queries.RelatorioRepassesMensais;
using PsicoFinance.Application.Features.Dashboard.Queries.RelatorioSessoesPeriodo;

namespace PsicoFinance.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly ISender _mediator;

    public DashboardController(ISender mediator) => _mediator = mediator;

    /// <summary>GET /api/dashboard/kpis?competencia=2025-03</summary>
    [HttpGet("kpis")]
    public async Task<IActionResult> ObterKpis(
        [FromQuery] string? competencia,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ObterKpisDashboardQuery(competencia), cancellationToken);
        return Ok(result);
    }

    /// <summary>GET /api/dashboard/relatorios/fluxo-caixa?inicio=2025-01&fim=2025-12</summary>
    [HttpGet("relatorios/fluxo-caixa")]
    public async Task<IActionResult> RelatorioFluxoCaixa(
        [FromQuery] string inicio,
        [FromQuery] string fim,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RelatorioFluxoCaixaQuery(inicio, fim), cancellationToken);
        return Ok(result);
    }

    /// <summary>GET /api/dashboard/relatorios/sessoes?dataInicio=2025-01-01&dataFim=2025-03-31</summary>
    [HttpGet("relatorios/sessoes")]
    public async Task<IActionResult> RelatorioSessoes(
        [FromQuery] DateOnly dataInicio,
        [FromQuery] DateOnly dataFim,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RelatorioSessoesPeriodoQuery(dataInicio, dataFim), cancellationToken);
        return Ok(result);
    }

    /// <summary>GET /api/dashboard/relatorios/repasses?inicio=2025-01&fim=2025-12</summary>
    [HttpGet("relatorios/repasses")]
    public async Task<IActionResult> RelatorioRepasses(
        [FromQuery] string? inicio,
        [FromQuery] string? fim,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RelatorioRepassesMensaisQuery(inicio, fim), cancellationToken);
        return Ok(result);
    }

    /// <summary>GET /api/dashboard/relatorios/inadimplencia?dataBase=2025-03-31</summary>
    [HttpGet("relatorios/inadimplencia")]
    public async Task<IActionResult> RelatorioInadimplencia(
        [FromQuery] DateOnly? dataBase,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RelatorioInadimplenciaQuery(dataBase), cancellationToken);
        return Ok(result);
    }
}
