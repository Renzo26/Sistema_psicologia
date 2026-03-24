using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Clinicas.DTOs;

namespace PsicoFinance.Application.Features.Clinicas.Queries.ObterClinica;

public class ObterClinicaQueryHandler : IRequestHandler<ObterClinicaQuery, ClinicaDto>
{
    private readonly IAppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public ObterClinicaQueryHandler(IAppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<ClinicaDto> Handle(ObterClinicaQuery request, CancellationToken cancellationToken)
    {
        var clinicaId = _tenantProvider.ClinicaId
            ?? throw new UnauthorizedAccessException("Tenant não identificado.");

        var clinica = await _context.Clinicas
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == clinicaId, cancellationToken)
            ?? throw new KeyNotFoundException("Clínica não encontrada.");

        return new ClinicaDto(
            clinica.Id,
            clinica.Nome,
            clinica.Cnpj,
            clinica.Email,
            clinica.Telefone,
            clinica.Cep,
            clinica.Logradouro,
            clinica.Numero,
            clinica.Complemento,
            clinica.Bairro,
            clinica.Cidade,
            clinica.Estado,
            clinica.HorarioEnvioAlerta,
            clinica.WebhookN8nUrl,
            clinica.Timezone,
            clinica.Ativo,
            clinica.CriadoEm);
    }
}
