using PsicoFinance.Domain.Common;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Domain.Entities;

public class Psicologo : TenantEntity
{
    public string Nome { get; set; } = null!;
    public string Crp { get; set; } = null!;
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? Cpf { get; set; }
    public TipoPsicologo Tipo { get; set; } = TipoPsicologo.Pj;

    public TipoRepasse TipoRepasse { get; set; } = TipoRepasse.Percentual;
    public decimal ValorRepasse { get; set; }

    public string? Banco { get; set; }
    public string? Agencia { get; set; }
    public string? Conta { get; set; }
    public string? PixChave { get; set; }
    public bool Ativo { get; set; } = true;

    // Navegação
    public Clinica Clinica { get; set; } = null!;
    public ICollection<Usuario> Usuarios { get; set; } = [];
    public ICollection<Contrato> Contratos { get; set; } = [];
}
