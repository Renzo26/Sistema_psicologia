using PsicoFinance.Domain.Common;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Domain.Entities;

public class NotaFiscal : TenantEntity
{
    public Guid PacienteId { get; set; }
    public Guid? LancamentoId { get; set; }

    // Dados da nota
    public string? NumeroNfse { get; set; }
    public string? CodigoVerificacao { get; set; }
    public DateTimeOffset? DataEmissao { get; set; }
    public DateOnly Competencia { get; set; }

    public decimal ValorServico { get; set; }
    public string DescricaoServico { get; set; } = "Serviços de Psicologia";

    public StatusNfse Status { get; set; } = StatusNfse.Pendente;
    public string? ErroMensagem { get; set; }

    // Arquivos
    public string? XmlUrl { get; set; }
    public string? PdfUrl { get; set; }

    // Resposta da API da prefeitura
    public string? RespostaApi { get; set; }

    public Guid? CriadoPor { get; set; }

    // Navegação
    public Clinica Clinica { get; set; } = null!;
    public Paciente Paciente { get; set; } = null!;
    public LancamentoFinanceiro? Lancamento { get; set; }
    public Usuario? CriadoPorUsuario { get; set; }
}
