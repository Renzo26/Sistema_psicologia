namespace PsicoFinance.Application.Common.Interfaces;

public interface ITenantProvider
{
    Guid? ClinicaId { get; }
    void SetClinicaId(Guid clinicaId);
}
