using FluentAssertions;
using PsicoFinance.Application.Features.Auth.Commands.Login;

namespace PsicoFinance.Tests.Auth;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Validate_DadosValidos_NaoTemErros()
    {
        var command = new LoginCommand("teste@email.com", "senha123", null, null);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmailVazio_TemErro()
    {
        var command = new LoginCommand("", "senha123", null, null);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validate_EmailInvalido_TemErro()
    {
        var command = new LoginCommand("nao-e-email", "senha123", null, null);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validate_SenhaVazia_TemErro()
    {
        var command = new LoginCommand("teste@email.com", "", null, null);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Senha");
    }
}
