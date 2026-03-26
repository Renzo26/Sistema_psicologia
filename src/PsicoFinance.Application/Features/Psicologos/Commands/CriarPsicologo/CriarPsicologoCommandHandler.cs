using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Psicologos.DTOs;
using PsicoFinance.Domain.Entities;

namespace PsicoFinance.Application.Features.Psicologos.Commands.CriarPsicologo;

public class CriarPsicologoCommandHandler : IRequestHandler<CriarPsicologoCommand, PsicologoDto>
{
    private readonly IAppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public CriarPsicologoCommandHandler(IAppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<PsicologoDto> Handle(CriarPsicologoCommand request, CancellationToken cancellationToken)
    {
        var clinicaId = _tenantProvider.ClinicaId
            ?? throw new UnauthorizedAccessException("Tenant não identificado.");

        // Verificar CRP duplicado na mesma clínica
        var crpExiste = await _context.Psicologos
            .AnyAsync(p => p.Crp == request.Crp, cancellationToken);

        if (crpExiste)
            throw new InvalidOperationException("Já existe um psicólogo com este CRP na clínica.");

        var psicologo = new Psicologo
        {
            Id = Guid.NewGuid(),
            ClinicaId = clinicaId,
            Nome = request.Nome,
            Crp = request.Crp,
            Email = request.Email,
            Telefone = request.Telefone,
            Cpf = request.Cpf,
            Tipo = request.Tipo,
            TipoRepasse = request.TipoRepasse,
            ValorRepasse = request.ValorRepasse,
            Banco = request.Banco,
            Agencia = request.Agencia,
            Conta = request.Conta,
            PixChave = request.PixChave,
            Ativo = true
        };

        _context.Psicologos.Add(psicologo);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(psicologo);
    }

    private static PsicologoDto MapToDto(Psicologo p) => new(
        p.Id, p.Nome, p.Crp, p.Email, p.Telefone, p.Cpf,
        p.Tipo, p.TipoRepasse, p.ValorRepasse,
        p.Banco, p.Agencia, p.Conta, p.PixChave,
        p.Ativo, p.CriadoEm);
}
