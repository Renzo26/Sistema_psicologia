using PsicoFinance.Application.Features.RelatoriosBI.DTOs;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.RelatoriosBI.Services;

public class RelatorioTemplatesService
{
    public List<RelatorioTemplateDto> ObterTemplates()
    {
        return
        [
            new RelatorioTemplateDto
            {
                Id = "tpl-financeiro-periodo",
                Nome = "Faturamento por Período",
                Descricao = "Agrupa receitas, despesas e saldo por competência mensal.",
                Tipo = TipoRelatorio.Financeiro,
                Agrupamento = "mes",
                FiltrosPadrao = new RelatorioFiltrosDto()
            },
            new RelatorioTemplateDto
            {
                Id = "tpl-produtividade-psicologo",
                Nome = "Produtividade por Psicólogo",
                Descricao = "Sessões realizadas, faltas, receita gerada e taxa de absenteísmo por psicólogo.",
                Tipo = TipoRelatorio.Psicologos,
                FiltrosPadrao = new RelatorioFiltrosDto()
            },
            new RelatorioTemplateDto
            {
                Id = "tpl-inadimplencia-aging",
                Nome = "Análise de Inadimplência — Aging",
                Descricao = "Lançamentos vencidos e não pagos agrupados por faixa: 0-30, 31-60, 61-90 e 90+ dias.",
                Tipo = TipoRelatorio.Inadimplencia,
                FiltrosPadrao = new RelatorioFiltrosDto()
            },
            new RelatorioTemplateDto
            {
                Id = "tpl-comparativo-mensal",
                Nome = "Comparativo Mensal",
                Descricao = "Comparação entre mês atual, mês anterior e mesmo mês do ano anterior.",
                Tipo = TipoRelatorio.Comparativo,
                FiltrosPadrao = new RelatorioFiltrosDto()
            },
            new RelatorioTemplateDto
            {
                Id = "tpl-ranking-pacientes",
                Nome = "Ranking de Pacientes por Receita",
                Descricao = "Receita total gerada, total de sessões e inadimplência por paciente.",
                Tipo = TipoRelatorio.Pacientes,
                FiltrosPadrao = new RelatorioFiltrosDto()
            },
            new RelatorioTemplateDto
            {
                Id = "tpl-repasses-detalhado",
                Nome = "Repasses por Psicólogo com Detalhamento",
                Descricao = "Valor de repasse mensal por psicólogo, total de sessões e status de pagamento.",
                Tipo = TipoRelatorio.Repasses,
                FiltrosPadrao = new RelatorioFiltrosDto()
            },
            new RelatorioTemplateDto
            {
                Id = "tpl-fluxo-projetado",
                Nome = "Fluxo de Caixa Projetado 30/60/90 dias",
                Descricao = "Entradas e saídas previstas e confirmadas para os próximos 90 dias.",
                Tipo = TipoRelatorio.FluxoCaixaProjetado,
                FiltrosPadrao = new RelatorioFiltrosDto()
            }
        ];
    }
}
