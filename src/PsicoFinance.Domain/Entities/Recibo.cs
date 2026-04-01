using PsicoFinance.Domain.Common;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Domain.Entities;

public class Recibo : TenantEntity
{
    public Guid SessaoId { get; set; }
    public Guid PacienteId { get; set; }
    public Guid? LancamentoId { get; set; }

    public string NumeroRecibo { get; set; } = null!;
    public decimal Valor { get; set; }
    public DateOnly DataEmissao { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public StatusRecibo Status { get; set; } = StatusRecibo.Gerado;

    // Armazenamento do arquivo
    public string? ArquivoUrl { get; set; }
    public string? ArquivoNome { get; set; }

    public Guid? CriadoPor { get; set; }

    // Navegação
    public Clinica Clinica { get; set; } = null!;
    public Sessao Sessao { get; set; } = null!;
    public Paciente Paciente { get; set; } = null!;
    public LancamentoFinanceiro? Lancamento { get; set; }
    public Usuario? CriadoPorUsuario { get; set; }
}
