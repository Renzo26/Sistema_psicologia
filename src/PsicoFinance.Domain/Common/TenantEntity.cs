namespace PsicoFinance.Domain.Common;

public abstract class TenantEntity : BaseEntity
{
    public Guid ClinicaId { get; set; }
}
