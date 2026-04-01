using MediatR;

namespace PsicoFinance.Application.Features.Recibos.Queries.ObterReciboPdf;

public record ObterReciboPdfQuery(Guid ReciboId) : IRequest<ObterReciboPdfResult>;

public record ObterReciboPdfResult(byte[] Content, string FileName);
