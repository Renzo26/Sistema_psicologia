using PsicoFinance.Domain.Common;

namespace PsicoFinance.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid ClinicaId { get; set; }
    public Guid? UsuarioId { get; set; }
    public string Acao { get; set; } = null!; // Criar, Atualizar, Excluir
    public string Entidade { get; set; } = null!; // Nome da entidade
    public Guid EntidadeId { get; set; }
    public string? DadosAnteriores { get; set; } // JSON snapshot
    public string? DadosNovos { get; set; } // JSON snapshot
    public string? IpOrigem { get; set; }
}
