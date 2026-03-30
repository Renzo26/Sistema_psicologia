using FluentValidation;

namespace PsicoFinance.Application.Features.Sessoes.Commands.AtualizarSessao;

public class AtualizarSessaoCommandValidator : AbstractValidator<AtualizarSessaoCommand>
{
    public AtualizarSessaoCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id da sessão é obrigatório.");

        RuleFor(x => x.Data)
            .NotEmpty().WithMessage("Data da sessão é obrigatória.");

        RuleFor(x => x.DuracaoMinutos)
            .InclusiveBetween(15, 240).WithMessage("Duração deve ser entre 15 e 240 minutos.");

        RuleFor(x => x.Observacoes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrWhiteSpace(x.Observacoes));
    }
}
