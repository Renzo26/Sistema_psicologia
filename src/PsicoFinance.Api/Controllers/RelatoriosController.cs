using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsicoFinance.Application.Features.Relatorios.Commands.GerarRelatorioMensal;
using PsicoFinance.Application.Features.Relatorios.DTOs;
using PsicoFinance.Application.Common.Interfaces;

namespace PsicoFinance.Api.Controllers;

[ApiController]
[Route("api/relatorios")]
[Authorize]
public class RelatoriosController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly IFileStorageService _storageService;

    public RelatoriosController(ISender mediator, IFileStorageService storageService)
    {
        _mediator = mediator;
        _storageService = storageService;
    }

    [HttpPost("mensal")]
    [Authorize(Roles = "Admin,Gerente")]
    [ProducesResponseType(typeof(RelatorioMensalDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GerarMensal([FromBody] GerarRelatorioMensalRequest request, CancellationToken ct)
    {
        var command = new GerarRelatorioMensalCommand(request.PsicologoId, request.Competencia);
        var result = await _mediator.Send(command, ct);
        return StatusCode(201, result);
    }

    [HttpGet("mensal/{*filePath}")]
    [ProducesResponseType(typeof(FileResult), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DownloadRelatorio(string filePath, CancellationToken ct)
    {
        var content = await _storageService.GetAsync(filePath, ct);
        if (content == null)
            return NotFound();

        var fileName = Path.GetFileName(filePath);
        return File(content, "application/pdf", fileName);
    }
}

public record GerarRelatorioMensalRequest(Guid PsicologoId, string Competencia);
