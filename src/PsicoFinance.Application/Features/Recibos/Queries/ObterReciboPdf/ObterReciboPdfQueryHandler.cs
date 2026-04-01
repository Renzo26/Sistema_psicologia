using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;

namespace PsicoFinance.Application.Features.Recibos.Queries.ObterReciboPdf;

public class ObterReciboPdfQueryHandler : IRequestHandler<ObterReciboPdfQuery, ObterReciboPdfResult>
{
    private readonly IAppDbContext _context;
    private readonly IFileStorageService _storageService;

    public ObterReciboPdfQueryHandler(IAppDbContext context, IFileStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<ObterReciboPdfResult> Handle(ObterReciboPdfQuery request, CancellationToken cancellationToken)
    {
        var recibo = await _context.Recibos
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.ReciboId, cancellationToken)
            ?? throw new KeyNotFoundException("Recibo não encontrado.");

        if (string.IsNullOrEmpty(recibo.ArquivoUrl))
            throw new InvalidOperationException("Arquivo PDF não disponível para este recibo.");

        var content = await _storageService.GetAsync(recibo.ArquivoUrl, cancellationToken)
            ?? throw new InvalidOperationException("Arquivo PDF não encontrado no storage.");

        var fileName = recibo.ArquivoNome ?? $"recibo_{recibo.NumeroRecibo}.pdf";

        return new ObterReciboPdfResult(content, fileName);
    }
}
