using FluentValidation;

namespace PsicoFinance.Application.Features.Sessoes.Commands.AgendarSessao;

public class AgendarSessaoCommandValidator : AbstractValidator<AgendarSessaoCommand>
{
    public AgendarSessaoCommandValidator()
    {
        RuleFor(x => x.ContratoId)
            .NotEmpty().WithMessage("Contrato é obrigatório.");

        RuleFor(x => x.Data)
            .NotEmpty().WithMessage("Data da sessão é obrigatória.");

        RuleFor(x => x.DuracaoMinutos)
            .InclusiveBetween(15, 240).WithMessage("Duração deve ser entre 15 e 240 minutos.")
            .When(x => x.DuracaoMinutos.HasValue);

        RuleFor(x => x.Observacoes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrWhiteSpace(x.Observacoes));
    }
}
