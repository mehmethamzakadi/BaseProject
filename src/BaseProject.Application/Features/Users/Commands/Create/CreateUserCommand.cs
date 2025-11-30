using BaseProject.Domain.Common.Results;
using MediatR;

namespace BaseProject.Application.Features.Users.Commands.Create;

public sealed record CreateUserCommand(string UserName, string Email, string Password) : IRequest<IResult>;
