using PsicoFinance.Domain.Common;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Domain.Entities;

public class Contrato : TenantEntity
{
    public Guid PacienteId { get; set; }
    public Guid PsicologoId { get; set; }

    public decimal ValorSessao { get; set; }
    public FormaPagamento FormaPagamento { get; set; } = FormaPagamento.Pix;
    public FrequenciaContrato Frequencia { get; set; } = FrequenciaContrato.Semanal;
    public DiaSemana DiaSemanasessao { get; set; }
    public TimeOnly HorarioSessao { get; set; }
    public int DuracaoMinutos { get; set; } = 50;

    public bool CobraFaltaInjustificada { get; set; } = true;
    public bool CobraFaltaJustificada { get; set; }

    public DateOnly DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }

    public StatusContrato Status { get; set; } = StatusContrato.Ativo;
    public string? MotivoEncerramento { get; set; }
    public Guid? PlanoContaId { get; set; }
    public string? Observacoes { get; set; }

    // Navegação
    public Clinica Clinica { get; set; } = null!;
    public Paciente Paciente { get; set; } = null!;
    public Psicologo Psicologo { get; set; } = null!;
    public PlanoConta? PlanoConta { get; set; }
}
