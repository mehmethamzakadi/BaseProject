using BaseProject.Domain.Common.Results;
using MediatR;

namespace BaseProject.Application.Features.Roles.Commands.Delete;

public sealed record DeleteRoleCommand(Guid Id) : IRequest<IResult>;
