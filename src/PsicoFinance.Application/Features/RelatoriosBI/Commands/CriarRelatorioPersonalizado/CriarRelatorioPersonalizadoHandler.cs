using System.Text.Json;
using MediatR;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.RelatoriosBI.DTOs;
using PsicoFinance.Domain.Entities;

namespace PsicoFinance.Application.Features.RelatoriosBI.Commands.CriarRelatorioPersonalizado;

public class CriarRelatorioPersonalizadoHandler : IRequestHandler<CriarRelatorioPersonalizadoCommand, RelatorioPersonalizadoDto>
{
    private readonly IAppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public CriarRelatorioPersonalizadoHandler(IAppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<RelatorioPersonalizadoDto> Handle(
        CriarRelatorioPersonalizadoCommand request,
        CancellationToken ct)
    {
        _ = _tenantProvider.ClinicaId
            ?? throw new UnauthorizedAccessException("Tenant não identificado.");

        var entidade = new RelatorioPersonalizado
        {
            Id = Guid.NewGuid(),
            Nome = request.Nome,
            Descricao = request.Descricao,
            Tipo = request.Tipo,
            FiltrosJson = JsonSerializer.Serialize(request.Filtros),
            Agrupamento = request.Agrupamento,
            Ordenacao = request.Ordenacao,
            Favorito = false,
            CriadoPorId = Guid.Empty
        };

        _context.RelatoriosPersonalizados.Add(entidade);
        await _context.SaveChangesAsync(ct);

        return MapToDto(entidade);
    }

    private static RelatorioPersonalizadoDto MapToDto(RelatorioPersonalizado e) => new()
    {
        Id = e.Id,
        Nome = e.Nome,
        Descricao = e.Descricao,
        Tipo = e.Tipo,
        FiltrosJson = e.FiltrosJson,
        Agrupamento = e.Agrupamento,
        Ordenacao = e.Ordenacao,
        Favorito = e.Favorito,
        CriadoPorId = e.CriadoPorId,
        CriadoEm = e.CriadoEm,
        AtualizadoEm = e.AtualizadoEm
    };
}
