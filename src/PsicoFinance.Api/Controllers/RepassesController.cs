using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsicoFinance.Application.Features.Repasses.Commands.GerarRepasseMensal;
using PsicoFinance.Application.Features.Repasses.Commands.PagarRepasse;
using PsicoFinance.Application.Features.Repasses.DTOs;
using PsicoFinance.Application.Features.Repasses.Queries.ListarRepasses;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Api.Controllers;

[ApiController]
[Route("api/repasses")]
[Authorize]
public class RepassesController : ControllerBase
{
    private readonly ISender _mediator;

    public RepassesController(ISender mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(List<RepasseDto>), 200)]
    public async Task<IActionResult> Listar(
        [FromQuery] string? mesReferencia,
        [FromQuery] Guid? psicologoId,
        [FromQuery] StatusRepasse? status,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new ListarRepassesQuery(mesReferencia, psicologoId, status), ct);
        return Ok(result);
    }

    [HttpPost("gerar")]
    [Authorize(Roles = "Admin,Gerente")]
    [ProducesResponseType(typeof(List<RepasseDto>), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Gerar([FromBody] GerarRepasseRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GerarRepasseMensalCommand(request.MesReferencia, request.PsicologoId), ct);
        return StatusCode(201, result);
    }

    [HttpPatch("{id:guid}/pagar")]
    [Authorize(Roles = "Admin,Gerente")]
    [ProducesResponseType(typeof(RepasseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Pagar(Guid id, [FromBody] PagarRepasseRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new PagarRepasseCommand(id, request.DataPagamento, request.Observacao), ct);
        return Ok(result);
    }
}

public record GerarRepasseRequest(string MesReferencia, Guid? PsicologoId);
public record PagarRepasseRequest(DateOnly DataPagamento, string? Observacao);
