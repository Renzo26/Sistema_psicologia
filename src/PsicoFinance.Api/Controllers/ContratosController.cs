using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsicoFinance.Application.Features.Contratos.Commands.AtualizarContrato;
using PsicoFinance.Application.Features.Contratos.Commands.CriarContrato;
using PsicoFinance.Application.Features.Contratos.Commands.EncerrarContrato;
using PsicoFinance.Application.Features.Contratos.DTOs;
using PsicoFinance.Application.Features.Contratos.Queries.ListarContratos;
using PsicoFinance.Application.Features.Contratos.Queries.ObterContrato;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Api.Controllers;

[ApiController]
[Route("api/contratos")]
[Authorize]
public class ContratosController : ControllerBase
{
    private readonly ISender _mediator;

    public ContratosController(ISender mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(List<ContratoResumoDto>), 200)]
    public async Task<IActionResult> Listar(
        [FromQuery] string? busca,
        [FromQuery] StatusContrato? status,
        [FromQuery] Guid? psicologoId,
        [FromQuery] Guid? pacienteId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new ListarContratosQuery(busca, status, psicologoId, pacienteId), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ContratoDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Obter(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ObterContratoQuery(id), ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Gerente,Secretaria")]
    [ProducesResponseType(typeof(ContratoDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Criar([FromBody] CriarContratoRequest request, CancellationToken ct)
    {
        var command = new CriarContratoCommand(
            request.PacienteId, request.PsicologoId,
            request.ValorSessao, request.FormaPagamento,
            request.Frequencia, request.DiaSemanaSessao,
            request.HorarioSessao, request.DuracaoMinutos,
            request.CobraFaltaInjustificada, request.CobraFaltaJustificada,
            request.DataInicio, request.DataFim,
            request.PlanoContaId, request.Observacoes);

        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(Obter), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Gerente,Secretaria")]
    [ProducesResponseType(typeof(ContratoDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarContratoRequest request, CancellationToken ct)
    {
        var command = new AtualizarContratoCommand(
            id, request.PacienteId, request.PsicologoId,
            request.ValorSessao, request.FormaPagamento,
            request.Frequencia, request.DiaSemanaSessao,
            request.HorarioSessao, request.DuracaoMinutos,
            request.CobraFaltaInjustificada, request.CobraFaltaJustificada,
            request.DataInicio, request.DataFim,
            request.PlanoContaId, request.Observacoes);

        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/encerrar")]
    [Authorize(Roles = "Admin,Gerente")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Encerrar(Guid id, [FromBody] EncerrarContratoRequest request, CancellationToken ct)
    {
        await _mediator.Send(new EncerrarContratoCommand(id, request.MotivoEncerramento), ct);
        return NoContent();
    }
}

public record CriarContratoRequest(
    Guid PacienteId, Guid PsicologoId,
    decimal ValorSessao, FormaPagamento FormaPagamento,
    FrequenciaContrato Frequencia, DiaSemana DiaSemanaSessao,
    TimeOnly HorarioSessao, int DuracaoMinutos,
    bool CobraFaltaInjustificada, bool CobraFaltaJustificada,
    DateOnly DataInicio, DateOnly? DataFim,
    Guid? PlanoContaId, string? Observacoes);

public record AtualizarContratoRequest(
    Guid PacienteId, Guid PsicologoId,
    decimal ValorSessao, FormaPagamento FormaPagamento,
    FrequenciaContrato Frequencia, DiaSemana DiaSemanaSessao,
    TimeOnly HorarioSessao, int DuracaoMinutos,
    bool CobraFaltaInjustificada, bool CobraFaltaJustificada,
    DateOnly DataInicio, DateOnly? DataFim,
    Guid? PlanoContaId, string? Observacoes);

public record EncerrarContratoRequest(string? MotivoEncerramento);
