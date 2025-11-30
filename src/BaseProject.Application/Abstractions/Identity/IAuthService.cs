using BaseProject.Application.Features.Auths.Login;
using BaseProject.Domain.Common.Results;

namespace BaseProject.Application.Abstractions.Identity;

public interface IAuthService
{
    Task<IDataResult<LoginResponse>> LoginAsync(string email, string password, string? deviceId = null);
    Task<IDataResult<LoginResponse>> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(string refreshToken);
    Task PasswordResetAsync(string email);
    Task<IDataResult<bool>> PasswordVerify(string resetToken, string userId);
}
