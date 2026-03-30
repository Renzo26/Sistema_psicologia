using PsicoFinance.Application.Common.Interfaces;

namespace PsicoFinance.Infrastructure.MultiTenancy;

public class TenantProvider : ITenantProvider
{
    public Guid? ClinicaId { get; private set; }
    public string? UserRole { get; private set; }

    public void SetClinicaId(Guid clinicaId) => ClinicaId = clinicaId;
    public void SetUserRole(string role) => UserRole = role;
}
