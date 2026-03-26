using FluentValidation;

namespace PsicoFinance.Application.Features.Pacientes.Commands.AtualizarPaciente;

public class AtualizarPacienteCommandValidator : AbstractValidator<AtualizarPacienteCommand>
{
    public AtualizarPacienteCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(150);

        RuleFor(x => x.Cpf)
            .MaximumLength(14)
            .When(x => !string.IsNullOrWhiteSpace(x.Cpf));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email inválido.")
            .MaximumLength(254)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Telefone).MaximumLength(20).When(x => !string.IsNullOrWhiteSpace(x.Telefone));

        RuleFor(x => x.DataNascimento)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .When(x => x.DataNascimento.HasValue);

        RuleFor(x => x.ResponsavelNome).MaximumLength(150).When(x => !string.IsNullOrWhiteSpace(x.ResponsavelNome));
        RuleFor(x => x.ResponsavelTelefone).MaximumLength(20).When(x => !string.IsNullOrWhiteSpace(x.ResponsavelTelefone));
        RuleFor(x => x.Observacoes).MaximumLength(2000).When(x => !string.IsNullOrWhiteSpace(x.Observacoes));
    }
}
