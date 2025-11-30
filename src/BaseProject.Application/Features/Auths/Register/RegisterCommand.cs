using BaseProject.Domain.Common.Results;
using MediatR;

namespace BaseProject.Application.Features.Auths.Register;

public sealed record RegisterCommand(string UserName, string Email, string Password) : IRequest<IResult>;
