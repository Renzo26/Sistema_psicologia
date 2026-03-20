using PsicoFinance.Domain.Common;

namespace PsicoFinance.Domain.Entities;

public class Paciente : TenantEntity
{
    public string Nome { get; set; } = null!;
    public string? Cpf { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public DateOnly? DataNascimento { get; set; }
    public string? ResponsavelNome { get; set; }
    public string? ResponsavelTelefone { get; set; }
    public string? Observacoes { get; set; }
    public bool Ativo { get; set; } = true;

    // Navegação
    public Clinica Clinica { get; set; } = null!;
    public ICollection<Contrato> Contratos { get; set; } = [];
}
