using PsicoFinance.Domain.Common;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Domain.Entities;

public class Sessao : TenantEntity
{
    public Guid ContratoId { get; set; }
    public Guid PacienteId { get; set; }
    public Guid PsicologoId { get; set; }

    public DateOnly Data { get; set; }
    public TimeOnly HorarioInicio { get; set; }
    public int DuracaoMinutos { get; set; } = 50;

    public StatusSessao Status { get; set; } = StatusSessao.Agendada;
    public string? Observacoes { get; set; }
    public string? MotivoFalta { get; set; }

    // Navegação
    public Clinica Clinica { get; set; } = null!;
    public Contrato Contrato { get; set; } = null!;
    public Paciente Paciente { get; set; } = null!;
    public Psicologo Psicologo { get; set; } = null!;
}
