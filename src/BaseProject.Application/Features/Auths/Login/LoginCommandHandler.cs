using BaseProject.Application.Abstractions.Identity;
using BaseProject.Domain.Common.Results;
using MediatR;

namespace BaseProject.Application.Features.Auths.Login;

public sealed class LoginCommandHandler(IAuthService authService) : IRequestHandler<LoginCommand, IDataResult<LoginResponse>>
{
    public async Task<IDataResult<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await authService.LoginAsync(request.Email, request.Password, request.DeviceId);
    }
}
