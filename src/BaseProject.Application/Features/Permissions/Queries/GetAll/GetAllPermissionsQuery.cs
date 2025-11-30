using BaseProject.Domain.Common.Results;
using MediatR;

namespace BaseProject.Application.Features.Permissions.Queries.GetAll;

public record GetAllPermissionsQuery : IRequest<IDataResult<GetAllPermissionsResponse>>;
