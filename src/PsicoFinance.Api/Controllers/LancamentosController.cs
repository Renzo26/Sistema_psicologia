using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsicoFinance.Application.Features.Lancamentos.Commands.AtualizarLancamento;
using PsicoFinance.Application.Features.Lancamentos.Commands.CancelarLancamento;
using PsicoFinance.Application.Features.Lancamentos.Commands.ConfirmarPagamento;
using PsicoFinance.Application.Features.Lancamentos.Commands.CriarLancamento;
using PsicoFinance.Application.Features.Lancamentos.DTOs;
using PsicoFinance.Application.Features.Lancamentos.Queries.ListarLancamentos;
using PsicoFinance.Application.Features.Lancamentos.Queries.ObterFluxoCaixa;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Api.Controllers;

[ApiController]
[Route("api/lancamentos")]
[Authorize]
public class LancamentosController : ControllerBase
{
    private readonly ISender _mediator;

    public LancamentosController(ISender mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(List<LancamentoDto>), 200)]
    public async Task<IActionResult> Listar(
        [FromQuery] string? competencia,
        [FromQuery] TipoLancamento? tipo,
        [FromQuery] StatusLancamento? status,
        [FromQuery] Guid? planoContaId,
        [FromQuery] DateOnly? dataInicio,
        [FromQuery] DateOnly? dataFim,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new ListarLancamentosQuery(competencia, tipo, status, planoContaId, dataInicio, dataFim), ct);
        return Ok(result);
    }

    [HttpGet("fluxo-caixa")]
    [ProducesResponseType(typeof(FluxoCaixaDto), 200)]
    public async Task<IActionResult> FluxoCaixa([FromQuery] string competencia, CancellationToken ct)
    {
        var result = await _mediator.Send(new ObterFluxoCaixaQuery(competencia), ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Gerente,Secretaria")]
    [ProducesResponseType(typeof(LancamentoDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Criar([FromBody] CriarLancamentoRequest request, CancellationToken ct)
    {
        var command = new CriarLancamentoCommand(
            request.Descricao, request.Valor, request.Tipo,
            request.DataVencimento, request.Competencia,
            request.PlanoContaId, request.SessaoId, request.Observacao);

        var result = await _mediator.Send(command, ct);
        return StatusCode(201, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Gerente,Secretaria")]
    [ProducesResponseType(typeof(LancamentoDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarLancamentoRequest request, CancellationToken ct)
    {
        var command = new AtualizarLancamentoCommand(
            id, request.Descricao, request.Valor, request.Tipo,
            request.DataVencimento, request.Competencia,
            request.PlanoContaId, request.Observacao);

        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/confirmar")]
    [Authorize(Roles = "Admin,Gerente,Secretaria")]
    [ProducesResponseType(typeof(LancamentoDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Confirmar(Guid id, [FromBody] ConfirmarPagamentoRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ConfirmarPagamentoCommand(id, request.DataPagamento), ct);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/cancelar")]
    [Authorize(Roles = "Admin,Gerente")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Cancelar(Guid id, [FromBody] CancelarLancamentoRequest request, CancellationToken ct)
    {
        await _mediator.Send(new CancelarLancamentoCommand(id, request.Motivo), ct);
        return NoContent();
    }
}

public record CriarLancamentoRequest(
    string Descricao, decimal Valor, TipoLancamento Tipo,
    DateOnly DataVencimento, string Competencia,
    Guid PlanoContaId, Guid? SessaoId, string? Observacao);

public record AtualizarLancamentoRequest(
    string Descricao, decimal Valor, TipoLancamento Tipo,
    DateOnly DataVencimento, string Competencia,
    Guid PlanoContaId, string? Observacao);

public record ConfirmarPagamentoRequest(DateOnly DataPagamento);
public record CancelarLancamentoRequest(string? Motivo);
