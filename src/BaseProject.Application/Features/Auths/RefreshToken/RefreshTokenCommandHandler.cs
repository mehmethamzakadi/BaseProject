using BaseProject.Application.Abstractions.Identity;
using BaseProject.Application.Features.Auths.Login;
using BaseProject.Domain.Common.Results;
using MediatR;

namespace BaseProject.Application.Features.Auths.RefreshToken;

public sealed class RefreshTokenCommandHandler(IAuthService authService) : IRequestHandler<RefreshTokenCommand, IDataResult<LoginResponse>>
{
    public async Task<IDataResult<LoginResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return await authService.RefreshTokenAsync(request.RefreshToken);
    }
}
