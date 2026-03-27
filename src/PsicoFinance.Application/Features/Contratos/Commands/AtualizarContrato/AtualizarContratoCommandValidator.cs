using FluentValidation;

namespace PsicoFinance.Application.Features.Contratos.Commands.AtualizarContrato;

public class AtualizarContratoCommandValidator : AbstractValidator<AtualizarContratoCommand>
{
    public AtualizarContratoCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id do contrato é obrigatório.");

        RuleFor(x => x.PacienteId)
            .NotEmpty().WithMessage("Paciente é obrigatório.");

        RuleFor(x => x.PsicologoId)
            .NotEmpty().WithMessage("Psicólogo é obrigatório.");

        RuleFor(x => x.ValorSessao)
            .GreaterThan(0).WithMessage("Valor da sessão deve ser maior que zero.");

        RuleFor(x => x.FormaPagamento)
            .IsInEnum().WithMessage("Forma de pagamento inválida.");

        RuleFor(x => x.Frequencia)
            .IsInEnum().WithMessage("Frequência inválida.");

        RuleFor(x => x.DiaSemanaSessao)
            .IsInEnum().WithMessage("Dia da semana inválido.");

        RuleFor(x => x.DuracaoMinutos)
            .InclusiveBetween(15, 240).WithMessage("Duração deve ser entre 15 e 240 minutos.");

        RuleFor(x => x.DataInicio)
            .NotEmpty().WithMessage("Data de início é obrigatória.");

        RuleFor(x => x.DataFim)
            .GreaterThan(x => x.DataInicio)
            .WithMessage("Data de término deve ser posterior à data de início.")
            .When(x => x.DataFim.HasValue);

        RuleFor(x => x.Observacoes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrWhiteSpace(x.Observacoes));
    }
}
