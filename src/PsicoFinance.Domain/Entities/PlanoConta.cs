using PsicoFinance.Domain.Common;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Domain.Entities;

public class PlanoConta : TenantEntity
{
    public string Nome { get; set; } = null!;
    public TipoPlanoConta Tipo { get; set; }
    public string? Descricao { get; set; }
    public bool Ativo { get; set; } = true;

    // Navegação
    public Clinica Clinica { get; set; } = null!;
    public ICollection<Contrato> Contratos { get; set; } = [];
}
