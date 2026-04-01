namespace PsicoFinance.Application.Common.Interfaces;

public interface IFileStorageService
{
    /// <summary>
    /// Salva um arquivo e retorna o caminho relativo para acesso.
    /// </summary>
    Task<string> SaveAsync(string folder, string fileName, byte[] content, CancellationToken ct = default);

    /// <summary>
    /// Obtém o conteúdo de um arquivo pelo caminho relativo.
    /// </summary>
    Task<byte[]?> GetAsync(string relativePath, CancellationToken ct = default);

    /// <summary>
    /// Remove um arquivo pelo caminho relativo.
    /// </summary>
    Task DeleteAsync(string relativePath, CancellationToken ct = default);
}
