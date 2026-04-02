using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.RelatoriosBI.Commands.AtualizarRelatorioPersonalizado;
using PsicoFinance.Application.Features.RelatoriosBI.Commands.CriarRelatorioPersonalizado;
using PsicoFinance.Application.Features.RelatoriosBI.Commands.ExcluirRelatorioPersonalizado;
using PsicoFinance.Application.Features.RelatoriosBI.Commands.MarcarFavorito;
using PsicoFinance.Application.Features.RelatoriosBI.DTOs;
using PsicoFinance.Application.Features.RelatoriosBI.Queries.ExecutarRelatorio;
using PsicoFinance.Application.Features.RelatoriosBI.Queries.ListarRelatoriosPersonalizados;
using PsicoFinance.Application.Features.RelatoriosBI.Queries.ObterRelatorioPersonalizado;
using PsicoFinance.Application.Features.RelatoriosBI.Services;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Api.Controllers;

[ApiController]
[Route("api/relatorios-bi")]
[Authorize]
public class RelatoriosBIController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly RelatorioTemplatesService _templatesService;
    private readonly IRelatorioExportService _exportService;

    public RelatoriosBIController(
        ISender mediator,
        RelatorioTemplatesService templatesService,
        IRelatorioExportService exportService)
    {
        _mediator = mediator;
        _templatesService = templatesService;
        _exportService = exportService;
    }

    // ── Listagem e templates ──────────────────────────────────────

    [HttpGet]
    [ProducesResponseType(typeof(List<RelatorioPersonalizadoDto>), 200)]
    public async Task<IActionResult> Listar(
        [FromQuery] TipoRelatorio? tipo,
        [FromQuery] bool? apenasFavorito,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new ListarRelatoriosPersonalizadosQuery(tipo, apenasFavorito), ct);
        return Ok(result);
    }

    [HttpGet("templates")]
    [ProducesResponseType(typeof(List<RelatorioTemplateDto>), 200)]
    public IActionResult ObterTemplates()
    {
        var templates = _templatesService.ObterTemplates();
        return Ok(templates);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RelatorioPersonalizadoDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ObterRelatorioPersonalizadoQuery(id), ct);
        return Ok(result);
    }

    // ── CRUD ──────────────────────────────────────────────────────

    [HttpPost]
    [Authorize(Roles = "Admin,Gerente")]
    [ProducesResponseType(typeof(RelatorioPersonalizadoDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Criar([FromBody] CriarRelatorioPersonalizadoCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(ObterPorId), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Gerente")]
    [ProducesResponseType(typeof(RelatorioPersonalizadoDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Atualizar(
        Guid id,
        [FromBody] AtualizarRelatorioPersonalizadoRequest request,
        CancellationToken ct)
    {
        var command = new AtualizarRelatorioPersonalizadoCommand(
            id, request.Nome, request.Descricao, request.Filtros, request.Agrupamento, request.Ordenacao);
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Gerente")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new ExcluirRelatorioPersonalizadoCommand(id), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/favorito")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> MarcarFavorito(Guid id, [FromBody] MarcarFavoritoRequest request, CancellationToken ct)
    {
        await _mediator.Send(new MarcarFavoritoCommand(id, request.Favorito), ct);
        return NoContent();
    }

    // ── Execução ──────────────────────────────────────────────────

    [HttpPost("executar")]
    [ProducesResponseType(typeof(RelatorioResultadoDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ExecutarAdHoc([FromBody] ExecutarRelatorioRequest request, CancellationToken ct)
    {
        var query = new ExecutarRelatorioQuery(null, request.Tipo, request.Filtros, request.Agrupamento, request.Ordenacao);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}/executar")]
    [ProducesResponseType(typeof(RelatorioResultadoDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ExecutarSalvo(Guid id, CancellationToken ct)
    {
        var relatorio = await _mediator.Send(new ObterRelatorioPersonalizadoQuery(id), ct);
        var filtros = System.Text.Json.JsonSerializer.Deserialize<RelatorioFiltrosDto>(relatorio.FiltrosJson)
                      ?? new RelatorioFiltrosDto();
        var query = new ExecutarRelatorioQuery(id, relatorio.Tipo, filtros, relatorio.Agrupamento, relatorio.Ordenacao);
        var result = await _mediator.Send(query, ct);
        return Ok(result);
    }

    // ── Exportação ────────────────────────────────────────────────

    [HttpGet("{id:guid}/exportar")]
    [ProducesResponseType(typeof(FileResult), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ExportarSalvo(
        Guid id,
        [FromQuery] FormatoExportacao formato,
        CancellationToken ct)
    {
        var relatorio = await _mediator.Send(new ObterRelatorioPersonalizadoQuery(id), ct);
        var filtros = System.Text.Json.JsonSerializer.Deserialize<RelatorioFiltrosDto>(relatorio.FiltrosJson)
                      ?? new RelatorioFiltrosDto();
        var query = new ExecutarRelatorioQuery(id, relatorio.Tipo, filtros, relatorio.Agrupamento, relatorio.Ordenacao);
        var resultado = await _mediator.Send(query, ct);

        return await GerarArquivoExportacao(resultado, formato, ct);
    }

    [HttpPost("exportar")]
    [ProducesResponseType(typeof(FileResult), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ExportarAdHoc([FromBody] ExportarRelatorioRequest request, CancellationToken ct)
    {
        var query = new ExecutarRelatorioQuery(null, request.Tipo, request.Filtros, null, null);
        var resultado = await _mediator.Send(query, ct);
        return await GerarArquivoExportacao(resultado, request.Formato, ct);
    }

    // ── Helpers ───────────────────────────────────────────────────

    private async Task<IActionResult> GerarArquivoExportacao(
        RelatorioResultadoDto resultado,
        FormatoExportacao formato,
        CancellationToken ct)
    {
        var nomeBase = resultado.Titulo.Replace(" ", "_").Replace("/", "-");

        return formato switch
        {
            FormatoExportacao.Pdf => File(
                await _exportService.ExportarPdfAsync(resultado, ct),
                "application/pdf",
                $"{nomeBase}.pdf"),
            FormatoExportacao.Xlsx => File(
                await _exportService.ExportarXlsxAsync(resultado, ct),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{nomeBase}.xlsx"),
            FormatoExportacao.Csv => File(
                _exportService.ExportarCsv(resultado),
                "text/csv",
                $"{nomeBase}.csv"),
            FormatoExportacao.Json => Ok(resultado) as IActionResult,
            _ => BadRequest("Formato de exportação inválido.")
        };
    }
}

// ── Request bodies ────────────────────────────────────────────────

public record AtualizarRelatorioPersonalizadoRequest(
    string Nome,
    string? Descricao,
    RelatorioFiltrosDto Filtros,
    string? Agrupamento,
    string? Ordenacao
);

public record MarcarFavoritoRequest(bool Favorito);

public record ExecutarRelatorioRequest(
    TipoRelatorio Tipo,
    RelatorioFiltrosDto Filtros,
    string? Agrupamento,
    string? Ordenacao
);

public record ExportarRelatorioRequest(
    TipoRelatorio Tipo,
    RelatorioFiltrosDto Filtros,
    FormatoExportacao Formato
);
