using BaseProject.Domain.Common.Requests;
using BaseProject.Domain.Common.Responses;
using MediatR;

namespace BaseProject.Application.Features.Users.Queries.GetList;

public sealed record GetListUsersQuery(PaginatedRequest PageRequest) : IRequest<PaginatedListResponse<GetListUserResponse>>;
