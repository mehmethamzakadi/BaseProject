using BaseProject.Domain.Common.Results;
using MediatR;

namespace BaseProject.Application.Features.Auths.Logout;

public sealed record LogoutCommand(string RefreshToken) : IRequest<IResult>;
