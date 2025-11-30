using BaseProject.Domain.Common.Results;
using MediatR;

namespace BaseProject.Application.Features.Roles.Commands.Create;

public sealed record CreateRoleCommand(string Name) : IRequest<IResult>;
