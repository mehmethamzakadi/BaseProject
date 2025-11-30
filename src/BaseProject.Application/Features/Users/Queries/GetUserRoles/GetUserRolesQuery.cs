using BaseProject.Domain.Common.Results;
using MediatR;

namespace BaseProject.Application.Features.Users.Queries.GetUserRoles;

public record GetUserRolesQuery(Guid UserId) : IRequest<IDataResult<GetUserRolesResponse>>;
