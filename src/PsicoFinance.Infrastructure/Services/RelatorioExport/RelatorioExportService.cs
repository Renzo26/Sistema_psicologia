using System.Text;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.RelatoriosBI.DTOs;

namespace PsicoFinance.Infrastructure.Services.RelatorioExport;

public class RelatorioExportService : IRelatorioExportService
{
    public Task<byte[]> ExportarPdfAsync(RelatorioResultadoDto resultado, CancellationToken ct)
    {
        var bytes = GerarPdf(resultado);
        return Task.FromResult(bytes);
    }

    public Task<byte[]> ExportarXlsxAsync(RelatorioResultadoDto resultado, CancellationToken ct)
    {
        var bytes = GerarXlsx(resultado);
        return Task.FromResult(bytes);
    }

    public byte[] ExportarCsv(RelatorioResultadoDto resultado)
        => GerarCsv(resultado);

    // ── PDF ───────────────────────────────────────────────────────

    private static byte[] GerarPdf(RelatorioResultadoDto resultado)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                page.Header().Element(h => ComposePdfHeader(h, resultado));
                page.Content().Element(c => ComposePdfTabela(c, resultado));
                page.Footer().Element(ComposePdfFooter);
            });
        });

        return document.GeneratePdf();
    }

    private static void ComposePdfHeader(IContainer container, RelatorioResultadoDto resultado)
    {
        container.BorderBottom(2).BorderColor(Colors.Indigo.Darken3).PaddingBottom(8).Column(col =>
        {
            col.Item().Text(resultado.Titulo).Bold().FontSize(14).FontColor(Colors.Indigo.Darken3);
            if (!string.IsNullOrEmpty(resultado.Descricao))
                col.Item().PaddingTop(2).Text(resultado.Descricao).FontSize(9).FontColor(Colors.Grey.Darken1);
            col.Item().PaddingTop(4).Text($"Gerado em: {resultado.GeradoEm:dd/MM/yyyy HH:mm} UTC")
                .FontSize(8).FontColor(Colors.Grey.Medium);
        });
    }

    private static void ComposePdfTabela(IContainer container, RelatorioResultadoDto resultado)
    {
        if (resultado.Colunas.Count == 0) return;

        container.PaddingTop(12).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                foreach (var _ in resultado.Colunas)
                    columns.RelativeColumn();
            });

            table.Header(header =>
            {
                foreach (var col in resultado.Colunas)
                {
                    header.Cell()
                        .Background(Colors.Indigo.Darken3)
                        .Padding(5)
                        .Text(col)
                        .Bold()
                        .FontSize(8)
                        .FontColor(Colors.White);
                }
            });

            var alternate = false;
            foreach (var linha in resultado.Linhas)
            {
                var bg = alternate ? Colors.Grey.Lighten4 : Colors.White;
                alternate = !alternate;

                foreach (var col in resultado.Colunas)
                {
                    linha.TryGetValue(col, out var valor);
                    var texto = FormatarValor(valor);
                    table.Cell().Background(bg).Padding(4).Text(texto).FontSize(8);
                }
            }
        });

        container.PaddingTop(8)
            .AlignRight()
            .Text($"Total de registros: {resultado.TotalRegistros}")
            .FontSize(8)
            .FontColor(Colors.Grey.Darken1);
    }

    private static void ComposePdfFooter(IContainer container)
    {
        container.BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(6).Row(row =>
        {
            row.RelativeItem().Text("Relatório gerado pelo PsicoFinance").FontSize(7).FontColor(Colors.Grey.Medium);
            row.ConstantItem(80).AlignRight().Text(text =>
            {
                text.Span("Página ").FontSize(7).FontColor(Colors.Grey.Medium);
                text.CurrentPageNumber().FontSize(7).FontColor(Colors.Grey.Medium);
                text.Span(" de ").FontSize(7).FontColor(Colors.Grey.Medium);
                text.TotalPages().FontSize(7).FontColor(Colors.Grey.Medium);
            });
        });
    }

    // ── XLSX ──────────────────────────────────────────────────────

    private static byte[] GerarXlsx(RelatorioResultadoDto resultado)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add(TruncarNome(resultado.Titulo));

        // Cabeçalho do relatório
        ws.Cell(1, 1).Value = resultado.Titulo;
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;
        ws.Range(1, 1, 1, resultado.Colunas.Count).Merge();

        if (!string.IsNullOrEmpty(resultado.Descricao))
        {
            ws.Cell(2, 1).Value = resultado.Descricao;
            ws.Cell(2, 1).Style.Font.FontColor = XLColor.Gray;
            ws.Range(2, 1, 2, resultado.Colunas.Count).Merge();
        }

        ws.Cell(3, 1).Value = $"Gerado em: {resultado.GeradoEm:dd/MM/yyyy HH:mm} UTC";
        ws.Cell(3, 1).Style.Font.Italic = true;
        ws.Cell(3, 1).Style.Font.FontSize = 8;

        // Linha de colunas
        var headerRow = 5;
        for (var i = 0; i < resultado.Colunas.Count; i++)
        {
            var cell = ws.Cell(headerRow, i + 1);
            cell.Value = resultado.Colunas[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromArgb(63, 81, 181);
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        // Dados
        for (var r = 0; r < resultado.Linhas.Count; r++)
        {
            var linha = resultado.Linhas[r];
            for (var c = 0; c < resultado.Colunas.Count; c++)
            {
                linha.TryGetValue(resultado.Colunas[c], out var valor);
                var cell = ws.Cell(headerRow + 1 + r, c + 1);
                AtribuirValorCelula(cell, valor);

                if (r % 2 == 1)
                    cell.Style.Fill.BackgroundColor = XLColor.FromArgb(245, 245, 250);
            }
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void AtribuirValorCelula(IXLCell cell, object? valor)
    {
        switch (valor)
        {
            case decimal d:
                cell.Value = d;
                cell.Style.NumberFormat.Format = "#,##0.00";
                break;
            case double dbl:
                cell.Value = dbl;
                cell.Style.NumberFormat.Format = "#,##0.00";
                break;
            case int i:
                cell.Value = i;
                break;
            case long l:
                cell.Value = l;
                break;
            case bool b:
                cell.Value = b;
                break;
            case null:
                cell.Value = string.Empty;
                break;
            default:
                cell.Value = valor.ToString();
                break;
        }
    }

    private static string TruncarNome(string nome)
        => nome.Length > 31 ? nome[..31] : nome;

    // ── CSV ───────────────────────────────────────────────────────

    private static byte[] GerarCsv(RelatorioResultadoDto resultado)
    {
        var sb = new StringBuilder();

        sb.AppendLine(string.Join(";", resultado.Colunas.Select(EscaparCsv)));

        foreach (var linha in resultado.Linhas)
        {
            var valores = resultado.Colunas.Select(col =>
            {
                linha.TryGetValue(col, out var valor);
                return EscaparCsv(FormatarValor(valor));
            });
            sb.AppendLine(string.Join(";", valores));
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string EscaparCsv(string valor)
    {
        if (valor.Contains(';') || valor.Contains('"') || valor.Contains('\n'))
            return $"\"{valor.Replace("\"", "\"\"")}\"";
        return valor;
    }

    private static string FormatarValor(object? valor)
    {
        return valor switch
        {
            null => string.Empty,
            decimal d => d.ToString("N2"),
            double dbl => dbl.ToString("N2"),
            _ => valor.ToString() ?? string.Empty
        };
    }
}
