using PsicoFinance.Domain.Common;

namespace PsicoFinance.Domain.Entities;

public class RefreshToken : TenantEntity
{
    public Guid UsuarioId { get; set; }
    public string TokenHash { get; set; } = null!;
    public DateTimeOffset ExpiraEm { get; set; }
    public bool Revogado { get; set; }
    public DateTimeOffset? RevogadoEm { get; set; }
    public string? IpOrigem { get; set; }
    public string? UserAgent { get; set; }

    // Navegação
    public Usuario Usuario { get; set; } = null!;
    public Clinica Clinica { get; set; } = null!;
}
