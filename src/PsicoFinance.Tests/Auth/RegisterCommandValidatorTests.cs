using FluentAssertions;
using PsicoFinance.Application.Features.Auth.Commands.Register;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Tests.Auth;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Fact]
    public void Validate_DadosValidos_NaoTemErros()
    {
        var command = new RegisterCommand(
            "Dr. Teste", "teste@email.com", "Senha123!", RoleUsuario.Admin, Guid.NewGuid(), null);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_NomeVazio_TemErro()
    {
        var command = new RegisterCommand(
            "", "teste@email.com", "Senha123!", RoleUsuario.Admin, Guid.NewGuid(), null);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Nome");
    }

    [Fact]
    public void Validate_SenhaCurta_TemErro()
    {
        var command = new RegisterCommand(
            "Teste", "teste@email.com", "Ab1", RoleUsuario.Admin, Guid.NewGuid(), null);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Senha");
    }

    [Fact]
    public void Validate_SenhaSemMaiuscula_TemErro()
    {
        var command = new RegisterCommand(
            "Teste", "teste@email.com", "senha123!", RoleUsuario.Admin, Guid.NewGuid(), null);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_SenhaSemMinuscula_TemErro()
    {
        var command = new RegisterCommand(
            "Teste", "teste@email.com", "SENHA123!", RoleUsuario.Admin, Guid.NewGuid(), null);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_SenhaSemNumero_TemErro()
    {
        var command = new RegisterCommand(
            "Teste", "teste@email.com", "SenhaAbc!", RoleUsuario.Admin, Guid.NewGuid(), null);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_ClinicaIdVazio_TemErro()
    {
        var command = new RegisterCommand(
            "Teste", "teste@email.com", "Senha123!", RoleUsuario.Admin, Guid.Empty, null);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ClinicaId");
    }
}
