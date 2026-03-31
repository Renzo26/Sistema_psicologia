using PsicoFinance.Domain.Common;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Domain.Entities;

public class FechamentoMensal : TenantEntity
{
    /// <summary>Formato YYYY-MM (ex: 2025-03)</summary>
    public string MesReferencia { get; set; } = null!;

    public StatusFechamento Status { get; set; } = StatusFechamento.Aberto;

    public decimal TotalReceitas { get; set; }
    public decimal TotalDespesas { get; set; }
    public decimal Saldo { get; set; }

    public int TotalSessoes { get; set; }
    public int TotalSessoesRealizadas { get; set; }
    public int TotalSessoesFalta { get; set; }

    public DateTimeOffset? FechadoEm { get; set; }
    public string? Observacao { get; set; }

    // Navegação
    public Clinica Clinica { get; set; } = null!;
}
