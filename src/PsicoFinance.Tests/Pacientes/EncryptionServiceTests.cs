using FluentAssertions;
using Microsoft.Extensions.Configuration;
using PsicoFinance.Infrastructure.Services.Encryption;

namespace PsicoFinance.Tests.Pacientes;

public class EncryptionServiceTests
{
    private readonly AesEncryptionService _service;

    public EncryptionServiceTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Encryption:Key"] = "TestEncryptionKeyForUnitTests!!"
            })
            .Build();

        _service = new AesEncryptionService(config);
    }

    [Fact]
    public void Encrypt_Decrypt_RetornaTextoOriginal()
    {
        var original = "123.456.789-00";
        var encrypted = _service.Encrypt(original);
        var decrypted = _service.Decrypt(encrypted);

        decrypted.Should().Be(original);
        encrypted.Should().NotBe(original);
    }

    [Fact]
    public void Encrypt_TextoVazio_RetornaVazio()
    {
        _service.Encrypt("").Should().Be("");
    }

    [Fact]
    public void Encrypt_Null_RetornaNull()
    {
        _service.Encrypt(null!).Should().BeNull();
    }

    [Fact]
    public void Encrypt_MesmoTexto_GeraResultadosDiferentes()
    {
        var text = "dados-sensiveis";
        var enc1 = _service.Encrypt(text);
        var enc2 = _service.Encrypt(text);

        // AES com IV aleatório gera cipher diferente cada vez
        enc1.Should().NotBe(enc2);

        // Mas ambos descriptografam para o mesmo valor
        _service.Decrypt(enc1).Should().Be(text);
        _service.Decrypt(enc2).Should().Be(text);
    }
}
