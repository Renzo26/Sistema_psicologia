namespace PsicoFinance.Application.Common.Interfaces;

public interface ITenantProvider
{
    Guid? ClinicaId { get; }
    string? UserRole { get; }
    void SetClinicaId(Guid clinicaId);
    void SetUserRole(string role);
}
