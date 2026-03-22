using PsicoFinance.Domain.Entities;

namespace PsicoFinance.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(Usuario usuario);
    string GenerateRefreshToken();
    Guid? ValidateAccessTokenAndGetUserId(string token);
}
