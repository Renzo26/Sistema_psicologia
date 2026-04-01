using Microsoft.Extensions.Configuration;
using PsicoFinance.Application.Common.Interfaces;

namespace PsicoFinance.Infrastructure.Services.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public LocalFileStorageService(IConfiguration configuration)
    {
        _basePath = configuration.GetValue<string>("FileStorage:BasePath")
            ?? Path.Combine(AppContext.BaseDirectory, "storage");
    }

    public async Task<string> SaveAsync(string folder, string fileName, byte[] content, CancellationToken ct = default)
    {
        var directoryPath = Path.Combine(_basePath, folder);
        Directory.CreateDirectory(directoryPath);

        var filePath = Path.Combine(directoryPath, fileName);
        await File.WriteAllBytesAsync(filePath, content, ct);

        // Retorna o caminho relativo para armazenar no banco
        return Path.Combine(folder, fileName).Replace("\\", "/");
    }

    public async Task<byte[]?> GetAsync(string relativePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_basePath, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));

        if (!File.Exists(fullPath))
            return null;

        return await File.ReadAllBytesAsync(fullPath, ct);
    }

    public Task DeleteAsync(string relativePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_basePath, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }
}
