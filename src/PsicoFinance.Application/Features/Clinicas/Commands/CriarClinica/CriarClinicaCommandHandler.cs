using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Clinicas.DTOs;
using PsicoFinance.Domain.Entities;

namespace PsicoFinance.Application.Features.Clinicas.Commands.CriarClinica;

public class CriarClinicaCommandHandler : IRequestHandler<CriarClinicaCommand, ClinicaDto>
{
    private readonly IAppDbContext _context;
    private readonly ITenantSchemaService _schemaService;
    private readonly ITenantProvider _tenantProvider;
    private readonly IAuditService _auditService;

    public CriarClinicaCommandHandler(
        IAppDbContext context,
        ITenantSchemaService schemaService,
        ITenantProvider tenantProvider,
        IAuditService auditService)
    {
        _context = context;
        _schemaService = schemaService;
        _tenantProvider = tenantProvider;
        _auditService = auditService;
    }

    public async Task<ClinicaDto> Handle(CriarClinicaCommand request, CancellationToken cancellationToken)
    {
        // Verificar CNPJ duplicado
        if (!string.IsNullOrWhiteSpace(request.Cnpj))
        {
            var cnpjExiste = await _context.Clinicas
                .AnyAsync(c => c.Cnpj == request.Cnpj, cancellationToken);

            if (cnpjExiste)
                throw new InvalidOperationException("Já existe uma clínica cadastrada com este CNPJ.");
        }

        // Verificar email duplicado
        var emailExiste = await _context.Clinicas
            .AnyAsync(c => c.Email == request.Email, cancellationToken);

        if (emailExiste)
            throw new InvalidOperationException("Já existe uma clínica cadastrada com este email.");

        var clinica = new Clinica
        {
            Id = Guid.NewGuid(),
            Nome = request.Nome,
            Cnpj = request.Cnpj,
            Email = request.Email,
            Telefone = request.Telefone,
            Cep = request.Cep,
            Logradouro = request.Logradouro,
            Numero = request.Numero,
            Complemento = request.Complemento,
            Bairro = request.Bairro,
            Cidade = request.Cidade,
            Estado = request.Estado,
            Ativo = true
        };

        _context.Clinicas.Add(clinica);
        await _context.SaveChangesAsync(cancellationToken);

        // Criar schema PostgreSQL para o novo tenant
        await _schemaService.CreateSchemaForTenantAsync(clinica.Id, cancellationToken);

        // Auditoria
        await _auditService.RegistrarAsync(new AuditLog
        {
            ClinicaId = clinica.Id,
            Acao = "Criar",
            Entidade = nameof(Clinica),
            EntidadeId = clinica.Id,
            DadosNovos = System.Text.Json.JsonSerializer.Serialize(new
            {
                request.Nome, request.Cnpj, request.Email,
                request.Telefone, request.Cidade, request.Estado
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
