using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Sessoes.Commands.GerarSessoesRecorrentes;
using PsicoFinance.Domain.Enums;
using PsicoFinance.Infrastructure.Persistence;

namespace PsicoFinance.Infrastructure.Jobs;

public class GerarSessoesMesSeguinteJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GerarSessoesMesSeguinteJob> _logger;

    public GerarSessoesMesSeguinteJob(
        IServiceScopeFactory scopeFactory,
        ILogger<GerarSessoesMesSeguinteJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        var proximo = DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(1);
        var inicioMes = new DateOnly(proximo.Year, proximo.Month, 1);
        var fimMes = inicioMes.AddMonths(1).AddDays(-1);

        _logger.LogInformation("Gerando sessões para {Mes}/{Ano}", proximo.Month, proximo.Year);

        using var scope = _scopeFactory.CreateScope();

        // Usa AppDbContext sem filtro de tenant para ler todos os contratos
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var contratos = await db.Contratos
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(c => c.Status == StatusContrato.Ativo
                     && c.ExcluidoEm == null
                     && (c.DataFim == null || c.DataFim >= inicioMes))
            .Select(c => new { c.Id, c.ClinicaId })
            .ToListAsync();

        _logger.LogInformation("{Count} contratos ativos encontrados", contratos.Count);

        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var tenantProvider = scope.ServiceProvider.GetRequiredService<ITenantProvider>();

        foreach (var contrato in contratos)
        {
            try
            {
                tenantProvider.SetClinicaId(contrato.ClinicaId);
                tenantProvider.SetUserRole("Admin");

                var command = new GerarSessoesRecorrentesCommand(
                    contrato.Id, inicioMes, fimMes, null);
                await sender.Send(command);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Erro ao gerar sessões para contrato {ContratoId}", contrato.Id);
            }
        }

        _logger.LogInformation("Job finalizado para {Mes}/{Ano}", proximo.Month, proximo.Year);
    }
}
