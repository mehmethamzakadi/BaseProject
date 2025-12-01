using AutoMapper;
using BaseProject.Domain.Common.Dynamic;
using BaseProject.Domain.Common.Paging;
using BaseProject.Domain.Common.Responses;
using BaseProject.Domain.Entities;
using BaseProject.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseProject.Application.Features.Users.Queries.GetPaginatedListByDynamic;

public sealed class GetPaginatedListByDynamicUsersQueryHandler(
    IUserRepository userRepository,
    IMapper mapper) : IRequestHandler<GetPaginatedListByDynamicUsersQuery, PaginatedListResponse<GetPaginatedListByDynamicUsersResponse>>
{
    public async Task<PaginatedListResponse<GetPaginatedListByDynamicUsersResponse>> Handle(GetPaginatedListByDynamicUsersQuery request, CancellationToken cancellationToken)
    {
        // ✅ Read-only sorgu - tracking'e gerek yok (performans için)
        Paginate<User> usersDynamic = await userRepository.GetPaginatedListByDynamicAsync(
            dynamic: request.DataGridRequest.DynamicQuery,
            index: request.DataGridRequest.PaginatedRequest.PageIndex,
            size: request.DataGridRequest.PaginatedRequest.PageSize,
            include: q => q.Include(u => u.UserRoles).ThenInclude(ur => ur.Role),
            enableTracking: false,
            cancellationToken: cancellationToken
        );

        PaginatedListResponse<GetPaginatedListByDynamicUsersResponse> response = mapper.Map<PaginatedListResponse<GetPaginatedListByDynamicUsersResponse>>(usersDynamic);

        return response;
    }
}
