using PsicoFinance.Domain.Common;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Domain.Entities;

public class LancamentoFinanceiro : TenantEntity
{
    public string Descricao { get; set; } = null!;
    public decimal Valor { get; set; }
    public TipoLancamento Tipo { get; set; }
    public StatusLancamento Status { get; set; } = StatusLancamento.Previsto;

    public DateOnly DataVencimento { get; set; }
    public DateOnly? DataPagamento { get; set; }

    /// <summary>Competência no formato YYYY-MM (ex: 2025-03)</summary>
    public string Competencia { get; set; } = null!;

    public Guid? SessaoId { get; set; }
    public Guid PlanoContaId { get; set; }
    public string? Observacao { get; set; }

    // Navegação
    public Clinica Clinica { get; set; } = null!;
    public Sessao? Sessao { get; set; }
    public PlanoConta PlanoConta { get; set; } = null!;
}
