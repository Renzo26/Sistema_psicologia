using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.RelatoriosBI.DTOs;

public class RelatorioPersonalizadoDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = null!;
    public string? Descricao { get; set; }
    public TipoRelatorio Tipo { get; set; }
    public string FiltrosJson { get; set; } = "{}";
    public string? Agrupamento { get; set; }
    public string? Ordenacao { get; set; }
    public bool Favorito { get; set; }
    public Guid CriadoPorId { get; set; }
    public DateTimeOffset CriadoEm { get; set; }
    public DateTimeOffset AtualizadoEm { get; set; }
}
