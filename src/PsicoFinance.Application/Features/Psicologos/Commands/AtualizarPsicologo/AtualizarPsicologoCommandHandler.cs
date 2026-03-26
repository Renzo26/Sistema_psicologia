using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Psicologos.DTOs;
using PsicoFinance.Domain.Entities;

namespace PsicoFinance.Application.Features.Psicologos.Commands.AtualizarPsicologo;

public class AtualizarPsicologoCommandHandler : IRequestHandler<AtualizarPsicologoCommand, PsicologoDto>
{
    private readonly IAppDbContext _context;

    public AtualizarPsicologoCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<PsicologoDto> Handle(AtualizarPsicologoCommand request, CancellationToken cancellationToken)
    {
        var psicologo = await _context.Psicologos
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Psicólogo não encontrado.");

        // Verificar CRP duplicado (excluindo o próprio)
        var crpExiste = await _context.Psicologos
            .AnyAsync(p => p.Crp == request.Crp && p.Id != request.Id, cancellationToken);

        if (crpExiste)
            throw new InvalidOperationException("Já existe outro psicólogo com este CRP na clínica.");

        psicologo.Nome = request.Nome;
        psicologo.Crp = request.Crp;
        psicologo.Email = request.Email;
        psicologo.Telefone = request.Telefone;
        psicologo.Cpf = request.Cpf;
        psicologo.Tipo = request.Tipo;
        psicologo.TipoRepasse = request.TipoRepasse;
        psicologo.ValorRepasse = request.ValorRepasse;
        psicologo.Banco = request.Banco;
        psicologo.Agencia = request.Agencia;
        psicologo.Conta = request.Conta;
        psicologo.PixChave = request.PixChave;

        await _context.SaveChangesAsync(cancellationToken);

        return new PsicologoDto(
            psicologo.Id, psicologo.Nome, psicologo.Crp, psicologo.Email,
            psicologo.Telefone, psicologo.Cpf, psicologo.Tipo,
            psicologo.TipoRepasse, psicologo.ValorRepasse,
            psicologo.Banco, psicologo.Agencia, psicologo.Conta, psicologo.PixChave,
            psicologo.Ativo, psicologo.CriadoEm);
    }
}
