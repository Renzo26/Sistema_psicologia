using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.RelatoriosBI.DTOs;

public class RelatorioFiltrosDto
{
    public DateOnly? DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
    public string? Competencia { get; set; }
    public Guid? PsicologoId { get; set; }
    public Guid? PacienteId { get; set; }
    public StatusSessao? StatusSessao { get; set; }
    public StatusLancamento? StatusLancamento { get; set; }
    public TipoLancamento? TipoLancamento { get; set; }
    public Guid? PlanoContaId { get; set; }
}
