using AutoMapper;
using BaseProject.Domain.Common.Paging;
using BaseProject.Domain.Common.Responses;
using BaseProject.Domain.Entities;
using BaseProject.Domain.Repositories;
using MediatR;

namespace BaseProject.Application.Features.Users.Queries.GetList;

public sealed class GetListUserQueryHandler(IUserRepository userRepository, IMapper mapper) : IRequestHandler<GetListUsersQuery, PaginatedListResponse<GetListUserResponse>>
{
    public async Task<PaginatedListResponse<GetListUserResponse>> Handle(GetListUsersQuery request, CancellationToken cancellationToken)
    {
        // ✅ Read-only sorgu - tracking'e gerek yok (performans için)
        // GetUsersAsync metodu zaten Include kullanıyor, tracking kapalı olmalı
        Paginate<User> userList = await userRepository.GetUsersAsync(
        index: request.PageRequest.PageIndex,
        size: request.PageRequest.PageSize,
        cancellationToken: cancellationToken
        );

        PaginatedListResponse<GetListUserResponse> response = mapper.Map<PaginatedListResponse<GetListUserResponse>>(userList);
        return response;
    }
}
