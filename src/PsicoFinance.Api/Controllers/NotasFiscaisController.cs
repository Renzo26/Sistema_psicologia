using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsicoFinance.Application.Features.NotasFiscais.Commands.CancelarNFSe;
using PsicoFinance.Application.Features.NotasFiscais.Commands.EmitirNFSe;
using PsicoFinance.Application.Features.NotasFiscais.DTOs;
using PsicoFinance.Application.Features.NotasFiscais.Queries.ListarNotasFiscais;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Api.Controllers;

[ApiController]
[Route("api/notas-fiscais")]
[Authorize]
public class NotasFiscaisController : ControllerBase
{
    private readonly ISender _mediator;

    public NotasFiscaisController(ISender mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(List<NotaFiscalDto>), 200)]
    public async Task<IActionResult> Listar(
        [FromQuery] Guid? pacienteId,
        [FromQuery] DateOnly? competenciaInicio,
        [FromQuery] DateOnly? competenciaFim,
        [FromQuery] StatusNfse? status,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new ListarNotasFiscaisQuery(pacienteId, competenciaInicio, competenciaFim, status), ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Gerente")]
    [ProducesResponseType(typeof(NotaFiscalDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Emitir([FromBody] EmitirNFSeRequest request, CancellationToken ct)
    {
        var command = new EmitirNFSeCommand(
            request.PacienteId, request.LancamentoId,
            request.ValorServico, request.DescricaoServico,
            request.Competencia);

        var result = await _mediator.Send(command, ct);
        return StatusCode(201, result);
    }

    [HttpPatch("{id:guid}/cancelar")]
    [Authorize(Roles = "Admin,Gerente")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Cancelar(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new CancelarNFSeCommand(id), ct);
        return NoContent();
    }
}

public record EmitirNFSeRequest(
    Guid PacienteId,
    Guid? LancamentoId,
    decimal ValorServico,
    string DescricaoServico,
    DateOnly Competencia);
