using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsicoFinance.Application.Features.Fechamentos.Commands.RealizarFechamentoMensal;
using PsicoFinance.Application.Features.Fechamentos.Queries.ListarFechamentos;
using PsicoFinance.Application.Features.Fechamentos.Queries.ObterFechamento;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Api.Controllers;

[ApiController]
[Route("api/fechamentos")]
[Authorize]
public class FechamentosController : ControllerBase
{
    private readonly ISender _mediator;

    public FechamentosController(ISender mediator) => _mediator = mediator;

    /// <summary>GET /api/fechamentos?status=Aberto</summary>
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] StatusFechamento? status,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ListarFechamentosQuery(status), cancellationToken);
        return Ok(result);
    }

    /// <summary>GET /api/fechamentos/{mes} onde mes = "2025-03"</summary>
    [HttpGet("{mes}")]
    public async Task<IActionResult> Obter(string mes, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ObterFechamentoQuery(mes), cancellationToken);
        return Ok(result);
    }

    /// <summary>POST /api/fechamentos — realiza o fechamento do mês</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Financeiro")]
    public async Task<IActionResult> RealizarFechamento(
        [FromBody] RealizarFechamentoMensalCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Obter), new { mes = result.MesReferencia }, result);
    }
}
