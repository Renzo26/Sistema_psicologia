using FluentValidation;

namespace PsicoFinance.Application.Features.Pacientes.Commands.CriarPaciente;

public class CriarPacienteCommandValidator : AbstractValidator<CriarPacienteCommand>
{
    public CriarPacienteCommandValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(150).WithMessage("Nome deve ter no máximo 150 caracteres.");

        RuleFor(x => x.Cpf)
            .MaximumLength(14).WithMessage("CPF deve ter no máximo 14 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Cpf));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email inválido.")
            .MaximumLength(254)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Telefone)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.Telefone));

        RuleFor(x => x.DataNascimento)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Data de nascimento não pode ser futura.")
            .When(x => x.DataNascimento.HasValue);

        RuleFor(x => x.ResponsavelNome)
            .MaximumLength(150)
            .When(x => !string.IsNullOrWhiteSpace(x.ResponsavelNome));

        RuleFor(x => x.ResponsavelTelefone)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.ResponsavelTelefone));

        RuleFor(x => x.Observacoes)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrWhiteSpace(x.Observacoes));
    }
}
