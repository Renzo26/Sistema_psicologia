using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsicoFinance.Application.Features.Recibos.Commands.CancelarRecibo;
using PsicoFinance.Application.Features.Recibos.Commands.EmitirRecibo;
using PsicoFinance.Application.Features.Recibos.DTOs;
using PsicoFinance.Application.Features.Recibos.Queries.ListarRecibos;
using PsicoFinance.Application.Features.Recibos.Queries.ObterReciboPdf;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Api.Controllers;

[ApiController]
[Route("api/recibos")]
[Authorize]
public class RecibosController : ControllerBase
{
    private readonly ISender _mediator;

    public RecibosController(ISender mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(List<ReciboDto>), 200)]
    public async Task<IActionResult> Listar(
        [FromQuery] Guid? pacienteId,
        [FromQuery] DateOnly? dataInicio,
        [FromQuery] DateOnly? dataFim,
        [FromQuery] StatusRecibo? status,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new ListarRecibosQuery(pacienteId, dataInicio, dataFim, status), ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Gerente,Secretaria")]
    [ProducesResponseType(typeof(ReciboDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Emitir([FromBody] EmitirReciboRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new EmitirReciboCommand(request.SessaoId), ct);
        return StatusCode(201, result);
    }

    [HttpGet("{id:guid}/pdf")]
    [ProducesResponseType(typeof(FileResult), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DownloadPdf(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ObterReciboPdfQuery(id), ct);
        return File(result.Content, "application/pdf", result.FileName);
    }

    [HttpPatch("{id:guid}/cancelar")]
    [Authorize(Roles = "Admin,Gerente")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Cancelar(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new CancelarReciboCommand(id), ct);
        return NoContent();
    }
}

public record EmitirReciboRequest(Guid SessaoId);
