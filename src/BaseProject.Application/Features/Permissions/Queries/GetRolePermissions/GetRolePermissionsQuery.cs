using BaseProject.Domain.Common.Results;
using MediatR;

namespace BaseProject.Application.Features.Permissions.Queries.GetRolePermissions;

public record GetRolePermissionsQuery(Guid RoleId) : IRequest<IDataResult<GetRolePermissionsResponse>>;
