using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsicoFinance.Application.Features.PlanosConta.Commands.AtualizarPlanoConta;
using PsicoFinance.Application.Features.PlanosConta.Commands.CriarPlanoConta;
using PsicoFinance.Application.Features.PlanosConta.Commands.ExcluirPlanoConta;
using PsicoFinance.Application.Features.PlanosConta.DTOs;
using PsicoFinance.Application.Features.PlanosConta.Queries.ListarPlanosConta;
using PsicoFinance.Application.Features.PlanosConta.Queries.ObterPlanoConta;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Api.Controllers;

[ApiController]
[Route("api/planos-conta")]
[Authorize]
public class PlanosContaController : ControllerBase
{
    private readonly ISender _mediator;

    public PlanosContaController(ISender mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(List<PlanoContaDto>), 200)]
    public async Task<IActionResult> Listar(
        [FromQuery] TipoPlanoConta? tipo,
        [FromQuery] bool? ativo,
        [FromQuery] string? busca,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new ListarPlanosContaQuery(tipo, ativo, busca), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PlanoContaDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Obter(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ObterPlanoContaQuery(id), ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Gerente")]
    [ProducesResponseType(typeof(PlanoContaDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Criar([FromBody] CriarPlanoContaRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CriarPlanoContaCommand(request.Nome, request.Tipo, request.Descricao), ct);
        return CreatedAtAction(nameof(Obter), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Gerente")]
    [ProducesResponseType(typeof(PlanoContaDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarPlanoContaRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new AtualizarPlanoContaCommand(id, request.Nome, request.Tipo, request.Descricao, request.Ativo), ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new ExcluirPlanoContaCommand(id), ct);
        return NoContent();
    }
}

public record CriarPlanoContaRequest(string Nome, TipoPlanoConta Tipo, string? Descricao);
public record AtualizarPlanoContaRequest(string Nome, TipoPlanoConta Tipo, string? Descricao, bool Ativo);
