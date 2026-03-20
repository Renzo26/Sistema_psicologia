namespace PsicoFinance.Infrastructure.MultiTenancy;

public interface ITenantProvider
{
    Guid? ClinicaId { get; }
    void SetClinicaId(Guid clinicaId);
}
