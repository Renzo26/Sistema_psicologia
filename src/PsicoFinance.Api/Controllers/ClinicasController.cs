using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsicoFinance.Application.Features.Clinicas.Commands.AtualizarClinica;
using PsicoFinance.Application.Features.Clinicas.DTOs;
using PsicoFinance.Application.Features.Clinicas.Queries.ObterClinica;

namespace PsicoFinance.Api.Controllers;

[ApiController]
[Route("api/clinicas")]
[Authorize]
public class ClinicasController : ControllerBase
{
    private readonly ISender _mediator;

    public ClinicasController(ISender mediator) => _mediator = mediator;

    /// <summary>
    /// Obtém os dados da clínica do tenant atual.
    /// </summary>
    [HttpGet("minha")]
    [ProducesResponseType(typeof(ClinicaDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ObterMinha(CancellationToken ct)
    {
        var result = await _mediator.Send(new ObterClinicaQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Atualiza os dados da clínica do tenant atual.
    /// </summary>
    [HttpPut("minha")]
    [ProducesResponseType(typeof(ClinicaDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Atualizar(
        [FromBody] AtualizarClinicaRequest request,
        CancellationToken ct)
    {
        var command = new AtualizarClinicaCommand(
            request.Nome,
            request.Cnpj,
            request.Email,
            request.Telefone,
            request.Cep,
            request.Logradouro,
            request.Numero,
            request.Complemento,
            request.Bairro,
            request.Cidade,
            request.Estado,
            request.HorarioEnvioAlerta,
            request.WebhookN8nUrl,
            request.Timezone);

        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }
}

public record AtualizarClinicaRequest(
    string Nome,
    string? Cnpj,
    string Email,
    string? Telefone,
    string? Cep,
    string? Logradouro,
    string? Numero,
    string? Complemento,
    string? Bairro,
    string? Cidade,
    string? Estado,
    TimeOnly HorarioEnvioAlerta,
    string? WebhookN8nUrl,
    string Timezone);
