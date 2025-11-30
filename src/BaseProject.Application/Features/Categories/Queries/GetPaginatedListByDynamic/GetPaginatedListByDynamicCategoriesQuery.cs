using BaseProject.Domain.Common.Requests;
using BaseProject.Domain.Common.Responses;
using MediatR;

namespace BaseProject.Application.Features.Categories.Queries.GetPaginatedListByDynamic;

public sealed record GetPaginatedListByDynamicCategoriesQuery(DataGridRequest DataGridRequest) : IRequest<PaginatedListResponse<GetPaginatedListByDynamicCategoriesResponse>>;
