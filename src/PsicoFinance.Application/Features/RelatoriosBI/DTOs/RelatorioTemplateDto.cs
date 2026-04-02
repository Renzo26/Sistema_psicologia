using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.RelatoriosBI.DTOs;

public class RelatorioTemplateDto
{
    public string Id { get; set; } = null!;
    public string Nome { get; set; } = null!;
    public string Descricao { get; set; } = null!;
    public TipoRelatorio Tipo { get; set; }
    public RelatorioFiltrosDto FiltrosPadrao { get; set; } = new();
    public string? Agrupamento { get; set; }
}
