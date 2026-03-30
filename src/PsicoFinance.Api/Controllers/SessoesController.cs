using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsicoFinance.Application.Features.Sessoes.Commands.AgendarSessao;
using PsicoFinance.Application.Features.Sessoes.Commands.AtualizarSessao;
using PsicoFinance.Application.Features.Sessoes.Commands.CancelarSessao;
using PsicoFinance.Application.Features.Sessoes.Commands.GerarSessoesRecorrentes;
using PsicoFinance.Application.Features.Sessoes.Commands.MarcarPresenca;
using PsicoFinance.Application.Features.Sessoes.Commands.RegistrarFalta;
using PsicoFinance.Application.Features.Sessoes.DTOs;
using PsicoFinance.Application.Features.Sessoes.Queries.ListarSessoes;
using PsicoFinance.Application.Features.Sessoes.Queries.ObterSessao;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Api.Controllers;

[ApiController]
[Route("api/sessoes")]
[Authorize]
public class SessoesController : ControllerBase
{
    private readonly ISender _mediator;

    public SessoesController(ISender mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(List<SessaoResumoDto>), 200)]
    public async Task<IActionResult> Listar(
        [FromQuery] DateOnly? dataInicio,
        [FromQuery] DateOnly? dataFim,
        [FromQuery] Guid? psicologoId,
        [FromQuery] Guid? pacienteId,
        [FromQuery] Guid? contratoId,
        [FromQuery] StatusSessao? status,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new ListarSessoesQuery(dataInicio, dataFim, psicologoId, pacienteId, contratoId, status), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SessaoDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Obter(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ObterSessaoQuery(id), ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Gerente,Secretaria")]
    [ProducesResponseType(typeof(SessaoDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Agendar([FromBody] AgendarSessaoRequest request, CancellationToken ct)
    {
        var command = new AgendarSessaoCommand(
            request.ContratoId, request.Data,
            request.HorarioInicio, request.DuracaoMinutos, request.Observacoes);

        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(Obter), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Gerente,Secretaria")]
    [ProducesResponseType(typeof(SessaoDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarSessaoRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new AtualizarSessaoCommand(id, request.Data, request.HorarioInicio, request.DuracaoMinutos, request.Observacoes), ct);
        return Ok(result);
    }

    [HttpPost("gerar-recorrentes")]
    [Authorize(Roles = "Admin,Gerente,Secretaria")]
    [ProducesResponseType(typeof(List<SessaoResumoDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GerarRecorrentes(
        [FromBody] GerarSessoesRecorrentesRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GerarSessoesRecorrentesCommand(
                request.ContratoId, request.DataInicio,
                request.DataFim, request.QuantidadeSessoes), ct);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/presenca")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> MarcarPresenca(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new MarcarPresencaCommand(id), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/falta")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RegistrarFalta(Guid id, [FromBody] RegistrarFaltaRequest request, CancellationToken ct)
    {
        await _mediator.Send(new RegistrarFaltaCommand(id, request.Justificada, request.Motivo), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/cancelar")]
    [Authorize(Roles = "Admin,Gerente,Secretaria")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Cancelar(Guid id, [FromBody] CancelarSessaoRequest request, CancellationToken ct)
    {
        await _mediator.Send(new CancelarSessaoCommand(id, request.Motivo), ct);
        return NoContent();
    }
}

public record AgendarSessaoRequest(
    Guid ContratoId,
    DateOnly Data,
    TimeOnly? HorarioInicio,
    int? DuracaoMinutos,
    string? Observacoes);

public record AtualizarSessaoRequest(
    DateOnly Data,
    TimeOnly HorarioInicio,
    int DuracaoMinutos,
    string? Observacoes);

public record GerarSessoesRecorrentesRequest(
    Guid ContratoId,
    DateOnly DataInicio,
    DateOnly? DataFim,
    int? QuantidadeSessoes);

public record RegistrarFaltaRequest(bool Justificada, string? Motivo);

public record CancelarSessaoRequest(string? Motivo);
