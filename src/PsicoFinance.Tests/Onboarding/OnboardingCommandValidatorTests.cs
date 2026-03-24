using FluentAssertions;
using PsicoFinance.Application.Features.Onboarding.Commands;

namespace PsicoFinance.Tests.Onboarding;

public class OnboardingCommandValidatorTests
{
    private readonly OnboardingCommandValidator _validator = new();

    private static OnboardingCommand CriarCommandValido() => new(
        NomeClinica: "Clínica Teste",
        Cnpj: "12.345.678/0001-00",
        EmailClinica: "contato@clinica.com",
        Telefone: "(11) 99999-0000",
        NomeAdmin: "Admin Teste",
        EmailAdmin: "admin@clinica.com",
        SenhaAdmin: "Senha123",
        IpOrigem: null,
        UserAgent: null);

    [Fact]
    public void Validate_DadosValidos_SemErros()
    {
        var result = _validator.Validate(CriarCommandValido());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_NomeClinicaVazio_RetornaErro()
    {
        var command = CriarCommandValido() with { NomeClinica = "" };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NomeClinica");
    }

    [Fact]
    public void Validate_EmailClinicaInvalido_RetornaErro()
    {
        var command = CriarCommandValido() with { EmailClinica = "nao-email" };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EmailClinica");
    }

    [Fact]
    public void Validate_NomeAdminVazio_RetornaErro()
    {
        var command = CriarCommandValido() with { NomeAdmin = "" };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NomeAdmin");
    }

    [Fact]
    public void Validate_EmailAdminInvalido_RetornaErro()
    {
        var command = CriarCommandValido() with { EmailAdmin = "invalido" };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EmailAdmin");
    }

    [Fact]
    public void Validate_SenhaCurta_RetornaErro()
    {
        var command = CriarCommandValido() with { SenhaAdmin = "Ab1" };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SenhaAdmin");
    }

    [Fact]
    public void Validate_SenhaSemMaiuscula_RetornaErro()
    {
        var command = CriarCommandValido() with { SenhaAdmin = "senha123" };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SenhaAdmin");
    }

    [Fact]
    public void Validate_SenhaSemMinuscula_RetornaErro()
    {
        var command = CriarCommandValido() with { SenhaAdmin = "SENHA123" };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SenhaAdmin");
    }

    [Fact]
    public void Validate_SenhaSemNumero_RetornaErro()
    {
        var command = CriarCommandValido() with { SenhaAdmin = "SenhaForte" };
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SenhaAdmin");
    }
}
