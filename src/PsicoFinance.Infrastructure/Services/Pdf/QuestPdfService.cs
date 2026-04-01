using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using PsicoFinance.Application.Common.Interfaces;

namespace PsicoFinance.Infrastructure.Services.Pdf;

public class QuestPdfService : IPdfService
{
    static QuestPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GerarRecibo(ReciboData data)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Element(h => ComposeReciboHeader(h, data));
                page.Content().Element(c => ComposeReciboContent(c, data));
                page.Footer().Element(ComposeReciboFooter);
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GerarRelatorioMensalPsicologo(RelatorioMensalData data)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Element(h => ComposeRelatorioHeader(h, data));
                page.Content().Element(c => ComposeRelatorioContent(c, data));
                page.Footer().Element(ComposeRelatorioFooter);
            });
        });

        return document.GeneratePdf();
    }

    // ── Recibo ────────────────────────────────────────────────────

    private static void ComposeReciboHeader(IContainer container, ReciboData data)
    {
        container.Column(col =>
        {
            col.Item().BorderBottom(2).BorderColor(Colors.Blue.Darken3).PaddingBottom(10).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text(data.ClinicaNome).Bold().FontSize(16).FontColor(Colors.Blue.Darken3);
                    if (!string.IsNullOrEmpty(data.ClinicaCnpj))
                        c.Item().Text($"CNPJ: {data.ClinicaCnpj}").FontSize(9).FontColor(Colors.Grey.Darken1);
                    if (!string.IsNullOrEmpty(data.ClinicaEndereco))
                        c.Item().Text(data.ClinicaEndereco).FontSize(9).FontColor(Colors.Grey.Darken1);
                    if (!string.IsNullOrEmpty(data.ClinicaTelefone))
                        c.Item().Text($"Tel: {data.ClinicaTelefone}").FontSize(9).FontColor(Colors.Grey.Darken1);
                });
                row.ConstantItem(160).AlignRight().Column(c =>
                {
                    c.Item().Background(Colors.Blue.Darken3).Padding(8).AlignCenter()
                        .Text("RECIBO").Bold().FontSize(14).FontColor(Colors.White);
                    c.Item().AlignCenter().PaddingTop(4)
                        .Text($"Nº {data.NumeroRecibo}").Bold().FontSize(11);
                });
            });
        });
    }

    private static void ComposeReciboContent(IContainer container, ReciboData data)
    {
        container.PaddingVertical(20).Column(col =>
        {
            col.Spacing(12);

            // Dados do paciente
            col.Item().Background(Colors.Grey.Lighten4).Padding(12).Column(inner =>
            {
                inner.Item().Text("DADOS DO PACIENTE").Bold().FontSize(9).FontColor(Colors.Blue.Darken3);
                inner.Item().PaddingTop(6).Row(row =>
                {
                    row.RelativeItem().Text($"Nome: {data.PacienteNome}");
                    if (!string.IsNullOrEmpty(data.PacienteCpf))
                        row.ConstantItem(200).Text($"CPF: {data.PacienteCpf}");
                });
            });

            // Dados do profissional
            col.Item().Background(Colors.Grey.Lighten4).Padding(12).Column(inner =>
            {
                inner.Item().Text("PROFISSIONAL RESPONSÁVEL").Bold().FontSize(9).FontColor(Colors.Blue.Darken3);
                inner.Item().PaddingTop(6).Row(row =>
                {
                    row.RelativeItem().Text($"Nome: {data.PsicologoNome}");
                    row.ConstantItem(200).Text($"CRP: {data.PsicologoCrp}");
                });
            });

            // Dados da sessão
            col.Item().Background(Colors.Grey.Lighten4).Padding(12).Column(inner =>
            {
                inner.Item().Text("DADOS DA SESSÃO").Bold().FontSize(9).FontColor(Colors.Blue.Darken3);
                inner.Item().PaddingTop(6).Row(row =>
                {
                    row.RelativeItem().Text($"Data: {data.DataSessao:dd/MM/yyyy}");
                    row.ConstantItem(150).Text($"Horário: {data.HorarioSessao:HH:mm}");
                    row.ConstantItem(150).Text($"Duração: {data.DuracaoMinutos} min");
                });
            });

            // Valor
            col.Item().Border(1).BorderColor(Colors.Blue.Darken3).Padding(15).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("VALOR RECEBIDO").Bold().FontSize(9).FontColor(Colors.Blue.Darken3);
                    c.Item().PaddingTop(4).Text($"Forma de pagamento: {data.FormaPagamento}");
                });
                row.ConstantItem(200).AlignRight().AlignMiddle()
                    .Text($"R$ {data.Valor:N2}").Bold().FontSize(18).FontColor(Colors.Blue.Darken3);
            });

            // Declaração
            col.Item().PaddingTop(10).Text(text =>
            {
                text.Span("Declaro ter recebido a importância acima referida, correspondente ao atendimento psicológico realizado na data indicada.");
                text.DefaultTextStyle(TextStyle.Default.FontSize(9).FontColor(Colors.Grey.Darken2));
            });

            // Data e assinatura
            col.Item().PaddingTop(30).Row(row =>
            {
                row.RelativeItem().Text($"Data de emissão: {data.DataEmissao:dd/MM/yyyy}");
            });

            col.Item().PaddingTop(40).AlignCenter().Column(assinatura =>
            {
                assinatura.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                assinatura.Item().PaddingTop(4).AlignCenter().Text(data.PsicologoNome).FontSize(10);
                assinatura.Item().AlignCenter().Text($"CRP {data.PsicologoCrp}").FontSize(9).FontColor(Colors.Grey.Darken1);
            });
        });
    }

    private static void ComposeReciboFooter(IContainer container)
    {
        container.BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(8).Row(row =>
        {
            row.RelativeItem().Text("Documento gerado eletronicamente pelo PsicoFinance")
                .FontSize(8).FontColor(Colors.Grey.Medium);
            row.ConstantItem(100).AlignRight().Text(text =>
            {
                text.Span("Página ").FontSize(8).FontColor(Colors.Grey.Medium);
                text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
            });
        });
    }

    // ── Relatório Mensal ──────────────────────────────────────────

    private static void ComposeRelatorioHeader(IContainer container, RelatorioMensalData data)
    {
        container.Column(col =>
        {
            col.Item().BorderBottom(2).BorderColor(Colors.Teal.Darken3).PaddingBottom(10).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text(data.ClinicaNome).Bold().FontSize(16).FontColor(Colors.Teal.Darken3);
                    if (!string.IsNullOrEmpty(data.ClinicaCnpj))
                        c.Item().Text($"CNPJ: {data.ClinicaCnpj}").FontSize(9).FontColor(Colors.Grey.Darken1);
                });
                row.ConstantItem(200).AlignRight().Column(c =>
                {
                    c.Item().Background(Colors.Teal.Darken3).Padding(8).AlignCenter()
                        .Text("RELATÓRIO MENSAL").Bold().FontSize(12).FontColor(Colors.White);
                    c.Item().AlignCenter().PaddingTop(4)
                        .Text(data.Competencia).Bold().FontSize(11);
                });
            });
        });
    }

    private static void ComposeRelatorioContent(IContainer container, RelatorioMensalData data)
    {
        container.PaddingVertical(20).Column(col =>
        {
            col.Spacing(12);

            // Dados do psicólogo
            col.Item().Background(Colors.Grey.Lighten4).Padding(12).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("PSICÓLOGO(A)").Bold().FontSize(9).FontColor(Colors.Teal.Darken3);
                    c.Item().PaddingTop(4).Text($"{data.PsicologoNome} — CRP {data.PsicologoCrp}");
                });
            });

            // Resumo de sessões
            col.Item().Text("RESUMO DE SESSÕES").Bold().FontSize(11).FontColor(Colors.Teal.Darken3);

            col.Item().Row(row =>
            {
                void KpiCard(string label, string value, string color)
                {
                    row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c =>
                    {
                        c.Item().AlignCenter().Text(label).FontSize(9).FontColor(Colors.Grey.Darken1);
                        c.Item().PaddingTop(4).AlignCenter().Text(value).Bold().FontSize(16);
                    });
                }

                KpiCard("Realizadas", data.TotalRealizadas.ToString(), "green");
                KpiCard("Faltas", data.TotalFaltas.ToString(), "orange");
                KpiCard("Canceladas", data.TotalCanceladas.ToString(), "red");
                KpiCard("Total", (data.TotalRealizadas + data.TotalFaltas + data.TotalCanceladas).ToString(), "blue");
            });

            // Tabela de sessões
            if (data.Sessoes.Count > 0)
            {
                col.Item().PaddingTop(8).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(90);  // Data
                        columns.ConstantColumn(70);  // Horário
                        columns.RelativeColumn();      // Paciente
                        columns.ConstantColumn(90);  // Status
                        columns.ConstantColumn(90);  // Valor
                    });

                    // Header
                    table.Header(header =>
                    {
                        void HeaderCell(string text)
                        {
                            header.Cell().Background(Colors.Teal.Darken3).Padding(6)
                                .Text(text).Bold().FontSize(9).FontColor(Colors.White);
                        }

                        HeaderCell("Data");
                        HeaderCell("Horário");
                        HeaderCell("Paciente");
                        HeaderCell("Status");
                        HeaderCell("Valor");
                    });

                    // Rows
                    var alternate = false;
                    foreach (var sessao in data.Sessoes)
                    {
                        var bgColor = alternate ? Colors.Grey.Lighten4 : Colors.White;
                        alternate = !alternate;

                        table.Cell().Background(bgColor).Padding(5).Text(sessao.Data.ToString("dd/MM/yyyy")).FontSize(9);
                        table.Cell().Background(bgColor).Padding(5).Text(sessao.Horario.ToString("HH:mm")).FontSize(9);
                        table.Cell().Background(bgColor).Padding(5).Text(sessao.PacienteNome).FontSize(9);
                        table.Cell().Background(bgColor).Padding(5).Text(sessao.Status).FontSize(9);
                        table.Cell().Background(bgColor).Padding(5).AlignRight()
                            .Text($"R$ {sessao.Valor:N2}").FontSize(9);
                    }
                });
            }

            // Resumo financeiro
            col.Item().PaddingTop(12).Text("RESUMO FINANCEIRO").Bold().FontSize(11).FontColor(Colors.Teal.Darken3);

            col.Item().Border(1).BorderColor(Colors.Teal.Darken3).Padding(15).Column(fin =>
            {
                fin.Spacing(6);
                fin.Item().Row(r =>
                {
                    r.RelativeItem().Text("Receita total das sessões:");
                    r.ConstantItem(120).AlignRight().Text($"R$ {data.ReceitaTotal:N2}").Bold();
                });
                fin.Item().Row(r =>
                {
                    var tipoLabel = data.TipoRepasse == "Percentual"
                        ? $"Repasse ({data.PercentualOuValorRepasse:N0}%)"
                        : $"Repasse (R$ {data.PercentualOuValorRepasse:N2}/sessão)";
                    r.RelativeItem().Text(tipoLabel);
                    r.ConstantItem(120).AlignRight().Text($"R$ {data.ValorRepasse:N2}").Bold().FontColor(Colors.Teal.Darken3);
                });
                fin.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                fin.Item().Row(r =>
                {
                    r.RelativeItem().Text("Receita líquida clínica:").Bold();
                    r.ConstantItem(120).AlignRight()
                        .Text($"R$ {(data.ReceitaTotal - data.ValorRepasse):N2}").Bold();
                });
            });
        });
    }

    private static void ComposeRelatorioFooter(IContainer container)
    {
        container.BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(8).Row(row =>
        {
            row.RelativeItem().Text("Relatório gerado eletronicamente pelo PsicoFinance")
                .FontSize(8).FontColor(Colors.Grey.Medium);
            row.ConstantItem(100).AlignRight().Text(text =>
            {
                text.Span("Página ").FontSize(8).FontColor(Colors.Grey.Medium);
                text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
            });
        });
    }
}
