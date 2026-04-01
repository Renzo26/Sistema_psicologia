using MediatR;
using PsicoFinance.Application.Features.Recibos.DTOs;

namespace PsicoFinance.Application.Features.Recibos.Commands.EmitirRecibo;

public record EmitirReciboCommand(Guid SessaoId) : IRequest<ReciboDto>;
