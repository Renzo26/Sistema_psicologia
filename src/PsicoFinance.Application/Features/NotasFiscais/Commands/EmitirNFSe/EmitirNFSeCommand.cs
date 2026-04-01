using MediatR;
using PsicoFinance.Application.Features.NotasFiscais.DTOs;

namespace PsicoFinance.Application.Features.NotasFiscais.Commands.EmitirNFSe;

public record EmitirNFSeCommand(
    Guid PacienteId,
    Guid? LancamentoId,
    decimal ValorServico,
    string DescricaoServico,
    DateOnly Competencia) : IRequest<NotaFiscalDto>;
