using PsicoFinance.Domain.Common;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Domain.Entities;

public class RelatorioPersonalizado : TenantEntity
{
    public string Nome { get; set; } = null!;
    public string? Descricao { get; set; }
    public TipoRelatorio Tipo { get; set; }

    /// <summary>JSON serializado dos filtros (RelatorioFiltrosDto)</summary>
    public string FiltrosJson { get; set; } = "{}";

    /// <summary>"dia" | "semana" | "mes" | "trimestre" | "ano"</summary>
    public string? Agrupamento { get; set; }

    /// <summary>"asc" | "desc"</summary>
    public string? Ordenacao { get; set; }

    public bool Favorito { get; set; }
    public Guid CriadoPorId { get; set; }

    // Navegação
    public Clinica Clinica { get; set; } = null!;
}
