using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Psicologos.DTOs;

namespace PsicoFinance.Application.Features.Psicologos.Queries.ObterPsicologo;

public class ObterPsicologoQueryHandler : IRequestHandler<ObterPsicologoQuery, PsicologoDto>
{
    private readonly IAppDbContext _context;

    public ObterPsicologoQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<PsicologoDto> Handle(ObterPsicologoQuery request, CancellationToken cancellationToken)
    {
        var p = await _context.Psicologos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Psicólogo não encontrado.");

        return new PsicologoDto(
            p.Id, p.Nome, p.Crp, p.Email, p.Telefone, p.Cpf,
            p.Tipo, p.TipoRepasse, p.ValorRepasse,
            p.Banco, p.Agencia, p.Conta, p.PixChave,
            p.Ativo, p.CriadoEm);
    }
}
