using MediatR;

namespace PsicoFinance.Application.Features.NotasFiscais.Commands.CancelarNFSe;

public record CancelarNFSeCommand(Guid NotaFiscalId) : IRequest;
