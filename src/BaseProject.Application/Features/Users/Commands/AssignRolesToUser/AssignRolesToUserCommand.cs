using BaseProject.Domain.Common.Results;
using MediatR;

namespace BaseProject.Application.Features.Users.Commands.AssignRolesToUser;

public class AssignRolesToUserCommand : IRequest<IResult>
{
    public Guid UserId { get; set; }
    public List<Guid> RoleIds { get; set; } = new();
}
