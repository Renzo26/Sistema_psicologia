using MediatR;

namespace PsicoFinance.Application.Features.Recibos.Commands.CancelarRecibo;

public record CancelarReciboCommand(Guid ReciboId) : IRequest;
