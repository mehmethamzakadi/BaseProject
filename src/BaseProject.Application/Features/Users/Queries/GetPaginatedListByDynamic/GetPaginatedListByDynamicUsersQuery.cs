using BaseProject.Domain.Common.Requests;
using BaseProject.Domain.Common.Responses;
using MediatR;

namespace BaseProject.Application.Features.Users.Queries.GetPaginatedListByDynamic;

public sealed record GetPaginatedListByDynamicUsersQuery(DataGridRequest DataGridRequest) : IRequest<PaginatedListResponse<GetPaginatedListByDynamicUsersResponse>>;
