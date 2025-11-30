using BaseProject.Domain.Common.Results;
using MediatR;

namespace BaseProject.Application.Features.Roles.Queries.GetRoleById;

public sealed record GetRoleByIdRequest(Guid Id) : IRequest<IDataResult<GetRoleByIdQueryResponse>>;
