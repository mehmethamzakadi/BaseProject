using BaseProject.Domain.Common.Requests;
using BaseProject.Domain.Common.Responses;
using MediatR;

namespace BaseProject.Application.Features.Roles.Queries.GetList;

public sealed record GetListRoleQuery(PaginatedRequest PageRequest) : IRequest<PaginatedListResponse<GetListRoleResponse>>;

