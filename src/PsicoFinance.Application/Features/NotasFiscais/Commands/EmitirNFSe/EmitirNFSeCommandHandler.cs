using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.NotasFiscais.DTOs;
using PsicoFinance.Domain.Entities;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.NotasFiscais.Commands.EmitirNFSe;

public class EmitirNFSeCommandHandler : IRequestHandler<EmitirNFSeCommand, NotaFiscalDto>
{
    private readonly IAppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public EmitirNFSeCommandHandler(IAppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<NotaFiscalDto> Handle(EmitirNFSeCommand request, CancellationToken cancellationToken)
    {
        var clinicaId = _tenantProvider.ClinicaId
            ?? throw new UnauthorizedAccessException("Tenant não identificado.");

        var paciente = await _context.Pacientes
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.PacienteId, cancellationToken)
            ?? throw new KeyNotFoundException("Paciente não encontrado.");

        // Cria a nota fiscal com status Pendente
        // A integração real com a API da prefeitura será feita futuramente
        var notaFiscal = new NotaFiscal
        {
            Id = Guid.NewGuid(),
            ClinicaId = clinicaId,
            PacienteId = request.PacienteId,
            LancamentoId = request.LancamentoId,
            ValorServico = request.ValorServico,
            DescricaoServico = request.DescricaoServico,
            Competencia = request.Competencia,
            Status = StatusNfse.Pendente,
            CriadoPor = null
        };

        _context.NotasFiscais.Add(notaFiscal);
        await _context.SaveChangesAsync(cancellationToken);

        return new NotaFiscalDto(
            notaFiscal.Id, notaFiscal.NumeroNfse,
            notaFiscal.PacienteId, paciente.Nome,
            notaFiscal.ValorServico, notaFiscal.DescricaoServico,
            notaFiscal.Competencia, notaFiscal.DataEmissao,
            notaFiscal.Status, notaFiscal.ErroMensagem,
            notaFiscal.PdfUrl, notaFiscal.CriadoEm);
    }
}
