using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsicoFinance.Application.Features.Pacientes.Commands.AtualizarPaciente;
using PsicoFinance.Application.Features.Pacientes.Commands.CriarPaciente;
using PsicoFinance.Application.Features.Pacientes.Commands.InativarPaciente;
using PsicoFinance.Application.Features.Pacientes.DTOs;
using PsicoFinance.Application.Features.Pacientes.Queries.ListarPacientes;
using PsicoFinance.Application.Features.Pacientes.Queries.ObterPaciente;

namespace PsicoFinance.Api.Controllers;

[ApiController]
[Route("api/pacientes")]
[Authorize]
public class PacientesController : ControllerBase
{
    private readonly ISender _mediator;

    public PacientesController(ISender mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(typeof(List<PacienteResumoDto>), 200)]
    public async Task<IActionResult> Listar(
        [FromQuery] string? busca,
        [FromQuery] bool? apenasAtivos,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new ListarPacientesQuery(busca, apenasAtivos ?? true), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PacienteDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Obter(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ObterPacienteQuery(id), ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Gerente,Secretaria")]
    [ProducesResponseType(typeof(PacienteDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Criar([FromBody] CriarPacienteRequest request, CancellationToken ct)
    {
        var command = new CriarPacienteCommand(
            request.Nome, request.Cpf, request.Email, request.Telefone,
            request.DataNascimento, request.ResponsavelNome,
            request.ResponsavelTelefone, request.Observacoes);

        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(Obter), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Gerente,Secretaria")]
    [ProducesResponseType(typeof(PacienteDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarPacienteRequest request, CancellationToken ct)
    {
        var command = new AtualizarPacienteCommand(
            id, request.Nome, request.Cpf, request.Email, request.Telefone,
            request.DataNascimento, request.ResponsavelNome,
            request.ResponsavelTelefone, request.Observacoes);

        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/inativar")]
    [Authorize(Roles = "Admin,Gerente")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Inativar(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new InativarPacienteCommand(id), ct);
        return NoContent();
    }
}

public record CriarPacienteRequest(
    string Nome, string? Cpf, string? Email, string? Telefone,
    DateOnly? DataNascimento, string? ResponsavelNome,
    string? ResponsavelTelefone, string? Observacoes);

public record AtualizarPacienteRequest(
    string Nome, string? Cpf, string? Email, string? Telefone,
    DateOnly? DataNascimento, string? ResponsavelNome,
    string? ResponsavelTelefone, string? Observacoes);
