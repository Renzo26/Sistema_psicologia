using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.RelatoriosBI.DTOs;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.RelatoriosBI.Queries.ExecutarRelatorio;

public class ExecutarRelatorioHandler : IRequestHandler<ExecutarRelatorioQuery, RelatorioResultadoDto>
{
    private readonly IAppDbContext _context;
    private readonly IEncryptionService _encryption;

    public ExecutarRelatorioHandler(IAppDbContext context, IEncryptionService encryption)
    {
        _context = context;
        _encryption = encryption;
    }

    public async Task<RelatorioResultadoDto> Handle(ExecutarRelatorioQuery request, CancellationToken ct)
    {
        return request.Tipo switch
        {
            TipoRelatorio.Financeiro => await ExecutarFinanceiro(request.Filtros, request.Agrupamento, ct),
            TipoRelatorio.Sessoes => await ExecutarSessoes(request.Filtros, ct),
            TipoRelatorio.Pacientes => await ExecutarPacientes(request.Filtros, ct),
            TipoRelatorio.Psicologos => await ExecutarPsicologos(request.Filtros, ct),
            TipoRelatorio.Inadimplencia => await ExecutarInadimplencia(request.Filtros, ct),
            TipoRelatorio.Repasses => await ExecutarRepasses(request.Filtros, ct),
            TipoRelatorio.FluxoCaixaProjetado => await ExecutarFluxoCaixaProjetado(request.Filtros, ct),
            TipoRelatorio.Comparativo => await ExecutarComparativo(request.Filtros, ct),
            _ => throw new ArgumentOutOfRangeException(nameof(request.Tipo))
        };
    }

    private async Task<RelatorioResultadoDto> ExecutarFinanceiro(
        RelatorioFiltrosDto filtros,
        string? agrupamento,
        CancellationToken ct)
    {
        var query = _context.LancamentosFinanceiros.AsNoTracking();

        if (filtros.DataInicio.HasValue)
            query = query.Where(l => l.DataVencimento >= filtros.DataInicio.Value);
        if (filtros.DataFim.HasValue)
            query = query.Where(l => l.DataVencimento <= filtros.DataFim.Value);
        if (!string.IsNullOrEmpty(filtros.Competencia))
            query = query.Where(l => l.Competencia == filtros.Competencia);
        if (filtros.StatusLancamento.HasValue)
            query = query.Where(l => l.Status == filtros.StatusLancamento.Value);
        if (filtros.TipoLancamento.HasValue)
            query = query.Where(l => l.Tipo == filtros.TipoLancamento.Value);
        if (filtros.PlanoContaId.HasValue)
            query = query.Where(l => l.PlanoContaId == filtros.PlanoContaId.Value);

        var dados = await query
            .GroupBy(l => l.Competencia)
            .Select(g => new
            {
                Competencia = g.Key,
                Receitas = g.Where(l => l.Tipo == TipoLancamento.Receita).Sum(l => l.Valor),
                Despesas = g.Where(l => l.Tipo == TipoLancamento.Despesa).Sum(l => l.Valor)
            })
            .OrderBy(g => g.Competencia)
            .ToListAsync(ct);

        var linhas = dados.Select(d => new Dictionary<string, object?>
        {
            ["Competência"] = d.Competencia,
            ["Receitas"] = d.Receitas,
            ["Despesas"] = d.Despesas,
            ["Saldo"] = d.Receitas - d.Despesas
        }).ToList();

        return new RelatorioResultadoDto
        {
            Titulo = "Relatório Financeiro",
            Descricao = "Receitas, despesas e saldo por competência",
            Colunas = ["Competência", "Receitas", "Despesas", "Saldo"],
            Linhas = linhas,
            TotalRegistros = linhas.Count,
            Agrupamento = agrupamento
        };
    }

    private async Task<RelatorioResultadoDto> ExecutarSessoes(
        RelatorioFiltrosDto filtros,
        CancellationToken ct)
    {
        var query = _context.Sessoes
            .AsNoTracking()
            .Include(s => s.Paciente)
            .Include(s => s.Psicologo)
            .Include(s => s.Contrato)
            .AsQueryable();

        if (filtros.DataInicio.HasValue)
            query = query.Where(s => s.Data >= filtros.DataInicio.Value);
        if (filtros.DataFim.HasValue)
            query = query.Where(s => s.Data <= filtros.DataFim.Value);
        if (filtros.PsicologoId.HasValue)
            query = query.Where(s => s.PsicologoId == filtros.PsicologoId.Value);
        if (filtros.PacienteId.HasValue)
            query = query.Where(s => s.PacienteId == filtros.PacienteId.Value);
        if (filtros.StatusSessao.HasValue)
            query = query.Where(s => s.Status == filtros.StatusSessao.Value);

        var sessoes = await query
            .OrderByDescending(s => s.Data)
            .ToListAsync(ct);

        var linhas = sessoes.Select(s => new Dictionary<string, object?>
        {
            ["Data"] = s.Data.ToString("dd/MM/yyyy"),
            ["Paciente"] = DecryptSafe(s.Paciente.Nome),
            ["Psicólogo"] = s.Psicologo.Nome,
            ["Status"] = s.Status.ToString(),
            ["Valor"] = s.Contrato?.ValorSessao ?? 0m
        }).ToList();

        return new RelatorioResultadoDto
        {
            Titulo = "Relatório de Sessões",
            Descricao = "Lista de sessões com paciente, psicólogo, status e valor",
            Colunas = ["Data", "Paciente", "Psicólogo", "Status", "Valor"],
            Linhas = linhas,
            TotalRegistros = linhas.Count
        };
    }

    private async Task<RelatorioResultadoDto> ExecutarPacientes(
        RelatorioFiltrosDto filtros,
        CancellationToken ct)
    {
        var dataInicio = filtros.DataInicio;
        var dataFim = filtros.DataFim;

        var query = _context.LancamentosFinanceiros
            .AsNoTracking()
            .Include(l => l.Sessao)
            .ThenInclude(s => s!.Paciente)
            .Where(l => l.Sessao != null && l.Tipo == TipoLancamento.Receita);

        if (dataInicio.HasValue)
            query = query.Where(l => l.DataVencimento >= dataInicio.Value);
        if (dataFim.HasValue)
            query = query.Where(l => l.DataVencimento <= dataFim.Value);

        var dados = await query.ToListAsync(ct);

        var agrupado = dados
            .GroupBy(l => l.Sessao!.PacienteId)
            .Select(g => new
            {
                PacienteId = g.Key,
                PacienteNome = g.First().Sessao!.Paciente.Nome,
                TotalSessoes = g.Select(l => l.SessaoId).Distinct().Count(),
                ReceitaTotal = g.Where(l => l.Status == StatusLancamento.Confirmado).Sum(l => l.Valor),
                Inadimplencia = g.Where(l => l.Status == StatusLancamento.Previsto
                    && l.DataVencimento < DateOnly.FromDateTime(DateTime.UtcNow)).Sum(l => l.Valor)
            })
            .OrderByDescending(g => g.ReceitaTotal)
            .ToList();

        var linhas = agrupado.Select(d => new Dictionary<string, object?>
        {
            ["Paciente"] = DecryptSafe(d.PacienteNome),
            ["TotalSessões"] = d.TotalSessoes,
            ["ReceitaTotal"] = d.ReceitaTotal,
            ["Inadimplência"] = d.Inadimplencia
        }).ToList();

        return new RelatorioResultadoDto
        {
            Titulo = "Ranking de Pacientes por Receita",
            Descricao = "Receita total, sessões e inadimplência por paciente",
            Colunas = ["Paciente", "TotalSessões", "ReceitaTotal", "Inadimplência"],
            Linhas = linhas,
            TotalRegistros = linhas.Count
        };
    }

    private async Task<RelatorioResultadoDto> ExecutarPsicologos(
        RelatorioFiltrosDto filtros,
        CancellationToken ct)
    {
        var queryBase = _context.Sessoes.AsNoTracking().Include(s => s.Psicologo);

        var query = queryBase.AsQueryable();

        if (filtros.DataInicio.HasValue)
            query = query.Where(s => s.Data >= filtros.DataInicio.Value);
        if (filtros.DataFim.HasValue)
            query = query.Where(s => s.Data <= filtros.DataFim.Value);
        if (filtros.PsicologoId.HasValue)
            query = query.Where(s => s.PsicologoId == filtros.PsicologoId.Value);

        var sessoes = await query.ToListAsync(ct);

        var agrupado = sessoes
            .GroupBy(s => s.PsicologoId)
            .Select(g =>
            {
                var total = g.Count();
                var realizadas = g.Count(s => s.Status == StatusSessao.Realizada);
                var faltas = g.Count(s => s.Status == StatusSessao.Falta || s.Status == StatusSessao.FaltaJustificada);
                return new
                {
                    PsicologoNome = g.First().Psicologo.Nome,
                    SessoesRealizadas = realizadas,
                    Faltas = faltas,
                    TaxaAbsenteismo = total > 0 ? Math.Round((double)faltas / total * 100, 2) : 0d
                };
            })
            .OrderByDescending(g => g.SessoesRealizadas)
            .ToList();

        var psicologoIds = sessoes.Select(s => s.PsicologoId).Distinct().ToList();
        var receitas = await _context.LancamentosFinanceiros
            .AsNoTracking()
            .Where(l => l.Sessao != null
                && psicologoIds.Contains(l.Sessao.PsicologoId)
                && l.Tipo == TipoLancamento.Receita
                && l.Status == StatusLancamento.Confirmado)
            .Include(l => l.Sessao)
            .GroupBy(l => l.Sessao!.PsicologoId)
            .Select(g => new { PsicologoId = g.Key, Receita = g.Sum(l => l.Valor) })
            .ToDictionaryAsync(g => g.PsicologoId, g => g.Receita, ct);

        var linhas = sessoes
            .GroupBy(s => s.PsicologoId)
            .Select(g =>
            {
                var total = g.Count();
                var realizadas = g.Count(s => s.Status == StatusSessao.Realizada);
                var faltas = g.Count(s => s.Status == StatusSessao.Falta || s.Status == StatusSessao.FaltaJustificada);
                var taxa = total > 0 ? Math.Round((double)faltas / total * 100, 2) : 0d;
                receitas.TryGetValue(g.Key, out var receita);
                return new Dictionary<string, object?>
                {
                    ["Psicólogo"] = g.First().Psicologo.Nome,
                    ["SessoesRealizadas"] = realizadas,
                    ["Faltas"] = faltas,
                    ["ReceitaGerada"] = receita,
                    ["TaxaAbsenteismo%"] = taxa
                };
            }).ToList();

        return new RelatorioResultadoDto
        {
            Titulo = "Produtividade por Psicólogo",
            Descricao = "Sessões realizadas, faltas, receita gerada e taxa de absenteísmo",
            Colunas = ["Psicólogo", "SessoesRealizadas", "Faltas", "ReceitaGerada", "TaxaAbsenteismo%"],
            Linhas = linhas,
            TotalRegistros = linhas.Count
        };
    }

    private async Task<RelatorioResultadoDto> ExecutarInadimplencia(
        RelatorioFiltrosDto filtros,
        CancellationToken ct)
    {
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);

        var query = _context.LancamentosFinanceiros
            .AsNoTracking()
            .Include(l => l.Sessao)
            .ThenInclude(s => s!.Paciente)
            .Where(l => l.Status == StatusLancamento.Previsto
                && l.DataVencimento < hoje
                && l.Tipo == TipoLancamento.Receita);

        if (filtros.PacienteId.HasValue)
            query = query.Where(l => l.Sessao != null && l.Sessao.PacienteId == filtros.PacienteId.Value);

        var dados = await query.ToListAsync(ct);

        var linhas = dados.Select(l =>
        {
            var dias = hoje.DayNumber - l.DataVencimento.DayNumber;
            var faixa = dias switch
            {
                <= 30 => "0-30 dias",
                <= 60 => "31-60 dias",
                <= 90 => "61-90 dias",
                _ => "90+ dias"
            };
            return new Dictionary<string, object?>
            {
                ["Paciente"] = l.Sessao != null ? DecryptSafe(l.Sessao.Paciente.Nome) : "N/A",
                ["Valor"] = l.Valor,
                ["DiasSemPagar"] = dias,
                ["Faixa"] = faixa,
                ["Vencimento"] = l.DataVencimento.ToString("dd/MM/yyyy")
            };
        }).OrderByDescending(d => (int)d["DiasSemPagar"]!).ToList();

        return new RelatorioResultadoDto
        {
            Titulo = "Análise de Inadimplência — Aging",
            Descricao = "Lançamentos vencidos e não pagos agrupados por faixa de atraso",
            Colunas = ["Paciente", "Valor", "DiasSemPagar", "Faixa", "Vencimento"],
            Linhas = linhas,
            TotalRegistros = linhas.Count
        };
    }

    private async Task<RelatorioResultadoDto> ExecutarRepasses(
        RelatorioFiltrosDto filtros,
        CancellationToken ct)
    {
        var query = _context.Repasses
            .AsNoTracking()
            .Include(r => r.Psicologo)
            .AsQueryable();

        if (filtros.PsicologoId.HasValue)
            query = query.Where(r => r.PsicologoId == filtros.PsicologoId.Value);
        if (!string.IsNullOrEmpty(filtros.Competencia))
            query = query.Where(r => r.MesReferencia == filtros.Competencia);

        var repasses = await query
            .OrderByDescending(r => r.MesReferencia)
            .ToListAsync(ct);

        var linhas = repasses.Select(r => new Dictionary<string, object?>
        {
            ["Psicólogo"] = r.Psicologo.Nome,
            ["Mês"] = r.MesReferencia,
            ["ValorRepasse"] = r.ValorCalculado,
            ["Status"] = r.Status.ToString()
        }).ToList();

        return new RelatorioResultadoDto
        {
            Titulo = "Repasses por Psicólogo",
            Descricao = "Valor de repasse mensal por psicólogo e status de pagamento",
            Colunas = ["Psicólogo", "Mês", "ValorRepasse", "Status"],
            Linhas = linhas,
            TotalRegistros = linhas.Count
        };
    }

    private async Task<RelatorioResultadoDto> ExecutarFluxoCaixaProjetado(
        RelatorioFiltrosDto filtros,
        CancellationToken ct)
    {
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var limite = hoje.AddDays(90);

        var lancamentos = await _context.LancamentosFinanceiros
            .AsNoTracking()
            .Where(l => l.DataVencimento >= hoje
                && l.DataVencimento <= limite
                && l.Status != StatusLancamento.Cancelado)
            .OrderBy(l => l.DataVencimento)
            .ToListAsync(ct);

        var agrupado = lancamentos
            .GroupBy(l => l.DataVencimento)
            .OrderBy(g => g.Key)
            .ToList();

        decimal saldoAcumulado = 0m;
        var linhas = new List<Dictionary<string, object?>>();

        foreach (var grupo in agrupado)
        {
            var previsto = grupo.Where(l => l.Status == StatusLancamento.Previsto)
                .Sum(l => l.Tipo == TipoLancamento.Receita ? l.Valor : -l.Valor);
            var confirmado = grupo.Where(l => l.Status == StatusLancamento.Confirmado)
                .Sum(l => l.Tipo == TipoLancamento.Receita ? l.Valor : -l.Valor);

            saldoAcumulado += confirmado;

            linhas.Add(new Dictionary<string, object?>
            {
                ["Data"] = grupo.Key.ToString("dd/MM/yyyy"),
                ["Previsto"] = previsto,
                ["Confirmado"] = confirmado,
                ["SaldoAcumulado"] = saldoAcumulado
            });
        }

        return new RelatorioResultadoDto
        {
            Titulo = "Fluxo de Caixa Projetado — 90 dias",
            Descricao = "Entradas e saídas previstas e confirmadas para os próximos 90 dias",
            Colunas = ["Data", "Previsto", "Confirmado", "SaldoAcumulado"],
            Linhas = linhas,
            TotalRegistros = linhas.Count
        };
    }

    private async Task<RelatorioResultadoDto> ExecutarComparativo(
        RelatorioFiltrosDto filtros,
        CancellationToken ct)
    {
        var hoje = DateTime.UtcNow;
        var mesAtual = $"{hoje.Year:D4}-{hoje.Month:D2}";
        var mesAnterior = $"{hoje.AddMonths(-1).Year:D4}-{hoje.AddMonths(-1).Month:D2}";
        var mesAnteriorAno = $"{hoje.AddYears(-1).Year:D4}-{hoje.Month:D2}";

        var competencias = new[] { mesAnteriorAno, mesAnterior, mesAtual };

        var dados = await _context.LancamentosFinanceiros
            .AsNoTracking()
            .Where(l => competencias.Contains(l.Competencia)
                && l.Status != StatusLancamento.Cancelado)
            .GroupBy(l => new { l.Competencia, l.Tipo })
            .Select(g => new { g.Key.Competencia, g.Key.Tipo, Total = g.Sum(l => l.Valor) })
            .ToListAsync(ct);

        var linhas = competencias.Select(comp =>
        {
            var receita = dados.Where(d => d.Competencia == comp && d.Tipo == TipoLancamento.Receita)
                .Sum(d => d.Total);
            var despesa = dados.Where(d => d.Competencia == comp && d.Tipo == TipoLancamento.Despesa)
                .Sum(d => d.Total);
            return new Dictionary<string, object?>
            {
                ["Mês"] = comp,
                ["Receita"] = receita,
                ["Despesa"] = despesa,
                ["Saldo"] = receita - despesa
            };
        }).ToList();

        return new RelatorioResultadoDto
        {
            Titulo = "Comparativo Mensal",
            Descricao = "Comparação entre mês atual, mês anterior e mesmo mês do ano anterior",
            Colunas = ["Mês", "Receita", "Despesa", "Saldo"],
            Linhas = linhas,
            TotalRegistros = linhas.Count
        };
    }

    private string DecryptSafe(string valor)
    {
        try { return _encryption.Decrypt(valor); }
        catch { return valor; }
    }
}
