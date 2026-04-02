namespace PsicoFinance.Application.Features.RelatoriosBI.DTOs;

public class RelatorioResultadoDto
{
    public string Titulo { get; set; } = null!;
    public string? Descricao { get; set; }
    public List<string> Colunas { get; set; } = [];
    public List<Dictionary<string, object?>> Linhas { get; set; } = [];
    public int TotalRegistros { get; set; }
    public string? Agrupamento { get; set; }
    public DateTimeOffset GeradoEm { get; set; } = DateTimeOffset.UtcNow;
}
