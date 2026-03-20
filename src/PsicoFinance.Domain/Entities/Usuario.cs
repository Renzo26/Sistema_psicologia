using PsicoFinance.Domain.Common;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Domain.Entities;

public class Usuario : TenantEntity
{
    public string Nome { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string SenhaHash { get; set; } = null!;
    public RoleUsuario Role { get; set; } = RoleUsuario.Secretaria;
    public Guid? PsicologoId { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTimeOffset? UltimoAcessoEm { get; set; }

    // Navegação
    public Clinica Clinica { get; set; } = null!;
    public Psicologo? Psicologo { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
