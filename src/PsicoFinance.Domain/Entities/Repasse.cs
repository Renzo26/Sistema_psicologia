using PsicoFinance.Domain.Common;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Domain.Entities;

public class Repasse : TenantEntity
{
    public Guid PsicologoId { get; set; }

    /// <summary>Referência no formato YYYY-MM (ex: 2025-03)</summary>
    public string MesReferencia { get; set; } = null!;

    public decimal ValorCalculado { get; set; }
    public int TotalSessoes { get; set; }
    public StatusRepasse Status { get; set; } = StatusRepasse.Pendente;
    public DateOnly? DataPagamento { get; set; }
    public string? Observacao { get; set; }

    // Navegação
    public Clinica Clinica { get; set; } = null!;
    public Psicologo Psicologo { get; set; } = null!;
}
