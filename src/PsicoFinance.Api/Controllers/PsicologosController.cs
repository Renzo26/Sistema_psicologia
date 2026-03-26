using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsicoFinance.Application.Features.Psicologos.Commands.AtualizarPsicologo;
using PsicoFinance.Application.Features.Psicologos.Commands.CriarPsicologo;
using PsicoFinance.Application.Features.Psicologos.Commands.InativarPsicologo;
using PsicoFinance.Application.Features.Psicologos.DTOs;
using PsicoFinance.Application.Features.Psicologos.Queries.ListarPsicologos;
using PsicoFinance.Application.Features.Psicologos.Queries.ObterPsicologo;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Api.Controllers;

[ApiController]
[Route("api/psicologos")]
[Authorize]
public class PsicologosController : ControllerBase
{
    private readonly ISender _mediator;

    public PsicologosController(ISender mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(List<PsicologoResumoDto>), 200)]
    public async Task<IActionResult> Listar(
        [FromQuery] string? busca,
        [FromQuery] bool? apenasAtivos,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new ListarPsicologosQuery(busca, apenasAtivos ?? true), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PsicologoDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Obter(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ObterPsicologoQuery(id), ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Gerente")]
    [ProducesResponseType(typeof(PsicologoDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Criar([FromBody] CriarPsicologoRequest request, CancellationToken ct)
    {
        var command = new CriarPsicologoCommand(
            request.Nome, request.Crp, request.Email, request.Telefone,
            request.Cpf, request.Tipo, request.TipoRepasse, request.ValorRepasse,
            request.Banco, request.Agencia, request.Conta, request.PixChave);

        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(Obter), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Gerente")]
    [ProducesResponseType(typeof(PsicologoDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarPsicologoRequest request, CancellationToken ct)
    {
        var command = new AtualizarPsicologoCommand(
            id, request.Nome, request.Crp, request.Email, request.Telefone,
            request.Cpf, request.Tipo, request.TipoRepasse, request.ValorRepasse,
            request.Banco, request.Agencia, request.Conta, request.PixChave);

        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/inativar")]
    [Authorize(Roles = "Admin,Gerente")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Inativar(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new InativarPsicologoCommand(id), ct);
        return NoContent();
    }
}

public record CriarPsicologoRequest(
    string Nome, string Crp, string? Email, string? Telefone, string? Cpf,
    TipoPsicologo Tipo, TipoRepasse TipoRepasse, decimal ValorRepasse,
    string? Banco, string? Agencia, string? Conta, string? PixChave);

public record AtualizarPsicologoRequest(
    string Nome, string Crp, string? Email, string? Telefone, string? Cpf,
    TipoPsicologo Tipo, TipoRepasse TipoRepasse, decimal ValorRepasse,
    string? Banco, string? Agencia, string? Conta, string? PixChave);
