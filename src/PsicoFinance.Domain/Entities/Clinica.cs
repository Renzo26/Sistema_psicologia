using PsicoFinance.Domain.Common;

namespace PsicoFinance.Domain.Entities;

public class Clinica : BaseEntity
{
    public string Nome { get; set; } = null!;
    public string? Cnpj { get; set; }
    public string Email { get; set; } = null!;
    public string? Telefone { get; set; }
    public string? Cep { get; set; }
    public string? Logradouro { get; set; }
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }

    public TimeOnly HorarioEnvioAlerta { get; set; } = new(8, 0);
    public string? WebhookN8nUrl { get; set; }
    public string Timezone { get; set; } = "America/Sao_Paulo";
    public bool Ativo { get; set; } = true;

    // Navegação
    public ICollection<Usuario> Usuarios { get; set; } = [];
    public ICollection<Psicologo> Psicologos { get; set; } = [];
    public ICollection<Paciente> Pacientes { get; set; } = [];
    public ICollection<Contrato> Contratos { get; set; } = [];
    public ICollection<PlanoConta> PlanosConta { get; set; } = [];
    public ICollection<Sessao> Sessoes { get; set; } = [];
}
