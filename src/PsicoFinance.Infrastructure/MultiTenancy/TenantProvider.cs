namespace PsicoFinance.Infrastructure.MultiTenancy;

public class TenantProvider : ITenantProvider
{
    public Guid? ClinicaId { get; private set; }

    public void SetClinicaId(Guid clinicaId)
    {
        ClinicaId = clinicaId;
    }
}
