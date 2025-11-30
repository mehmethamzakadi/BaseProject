using BaseProject.Application.Abstractions.Identity;
using BaseProject.Domain.Common.Results;
using MediatR;

namespace BaseProject.Application.Features.Auths.Logout;

public sealed class LogoutCommandHandler(IAuthService authService) : IRequestHandler<LogoutCommand, IResult>
{
    public async Task<IResult> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        await authService.LogoutAsync(request.RefreshToken);
        return new SuccessResult("Çıkış yapıldı.");
    }
}
