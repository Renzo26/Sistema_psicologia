using FluentValidation;

namespace PsicoFinance.Application.Features.NotasFiscais.Commands.EmitirNFSe;

public class EmitirNFSeCommandValidator : AbstractValidator<EmitirNFSeCommand>
{
    public EmitirNFSeCommandValidator()
    {
        RuleFor(x => x.PacienteId)
            .NotEmpty().WithMessage("Paciente é obrigatório.");

        RuleFor(x => x.ValorServico)
            .GreaterThan(0).WithMessage("Valor do serviço deve ser maior que zero.");

        RuleFor(x => x.DescricaoServico)
            .NotEmpty().WithMessage("Descrição do serviço é obrigatória.")
            .MaximumLength(500).WithMessage("Descrição deve ter no máximo 500 caracteres.");
    }
}
