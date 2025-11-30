using BaseProject.Domain.Common.Results;
using MediatR;

namespace BaseProject.Application.Features.Users.Commands.Update;

public sealed record UpdateUserCommand(Guid Id, string UserName, string Email) : IRequest<IResult>;
