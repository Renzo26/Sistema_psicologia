using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Clinicas.DTOs;
using PsicoFinance.Domain.Entities;

namespace PsicoFinance.Application.Features.Clinicas.Commands.AtualizarClinica;

public class AtualizarClinicaCommandHandler : IRequestHandler<AtualizarClinicaCommand, ClinicaDto>
{
    private readonly IAppDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly IAuditService _auditService;

    public AtualizarClinicaCommandHandler(
        IAppDbContext context,
        ITenantProvider tenantProvider,
        IAuditService auditService)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _auditService = auditService;
    }

    public async Task<ClinicaDto> Handle(AtualizarClinicaCommand request, CancellationToken cancellationToken)
    {
        var clinicaId = _tenantProvider.ClinicaId
            ?? throw new UnauthorizedAccessException("Tenant não identificado.");

        var clinica = await _context.Clinicas
            .FirstOrDefaultAsync(c => c.Id == clinicaId, cancellationToken)
            ?? throw new KeyNotFoundException("Clínica não encontrada.");

        // Snapshot dos dados anteriores para auditoria
        var dadosAnteriores = new
        {
            clinica.Nome, clinica.Cnpj, clinica.Email, clinica.Telefone,
            clinica.Cep, clinica.Logradouro, clinica.Numero, clinica.Complemento,
            clinica.Bairro, clinica.Cidade, clinica.Estado,
            clinica.HorarioEnvioAlerta, clinica.WebhookN8nUrl, clinica.Timezone
        };

        // Atualizar campos
        clinica.Nome = request.Nome;
        clinica.Cnpj = request.Cnpj;
        clinica.Email = request.Email;
        clinica.Telefone = request.Telefone;
        clinica.Cep = request.Cep;
        clinica.Logradouro = request.Logradouro;
        clinica.Numero = request.Numero;
        clinica.Complemento = request.Complemento;
        clinica.Bairro = request.Bairro;
        clinica.Cidade = request.Cidade;
        clinica.Estado = request.Estado;
        clinica.HorarioEnvioAlerta = request.HorarioEnvioAlerta;
        clinica.WebhookN8nUrl = request.WebhookN8nUrl;
        clinica.Timezone = request.Timezone;

        await _context.SaveChangesAsync(cancellationToken);

        // Auditoria
        await _auditService.RegistrarAsync(new AuditLog
        {
            ClinicaId = clinicaId,
            Acao = "Atualizar",
            Entidade = nameof(Clinica),
            EntidadeId = clinicaId,
            DadosAnteriores = System.Text.Json.JsonSerializer.Serialize(dadosAnteriores),
            DadosNovos = System.Text.Json.JsonSerializer.Serialize(new
            {
                request.Nome, request.Cnpj, request.Email, request.Telefone,
                request.Cep, request.Logradouro, request.Numero, request.Complemento,
                request.Bairro, request.Cidade, request.Estado,
                request.HorarioEnvioAlerta, request.WebhookN8nUrl, request.Timezone
            })
        }, cancellationToken);

        return new ClinicaDto(
            clinica.Id, clinica.Nome, clinica.Cnpj, clinica.Email, clinica.Telefone,
            clinica.Cep, clinica.Logradouro, clinica.Numero, clinica.Complemento,
            clinica.Bairro, clinica.Cidade, clinica.Estado,
            clinica.HorarioEnvioAlerta, clinica.WebhookN8nUrl, clinica.Timezone,
            clinica.Ativo, clinica.CriadoEm);
    }
}
