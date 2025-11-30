using BaseProject.Application.Features.Auths.Login;
using BaseProject.Domain.Common.Results;
using MediatR;

namespace BaseProject.Application.Features.Auths.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<IDataResult<LoginResponse>>;
