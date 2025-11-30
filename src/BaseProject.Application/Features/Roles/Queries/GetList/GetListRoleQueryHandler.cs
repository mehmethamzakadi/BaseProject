using AutoMapper;
using BaseProject.Domain.Common.Responses;
using BaseProject.Domain.Repositories;
using MediatR;


namespace BaseProject.Application.Features.Roles.Queries.GetList;

public sealed class GetListRoleQueryHandler(IRoleRepository roleRepository, IMapper mapper) : IRequestHandler<GetListRoleQuery, PaginatedListResponse<GetListRoleResponse>>
{

    public async Task<PaginatedListResponse<GetListRoleResponse>> Handle(GetListRoleQuery request, CancellationToken cancellationToken)
    {
        var roles = await roleRepository.GetRoles(
            index: request.PageRequest.PageIndex,
            size: request.PageRequest.PageSize,
            cancellationToken: cancellationToken);

        PaginatedListResponse<GetListRoleResponse> response = mapper.Map<PaginatedListResponse<GetListRoleResponse>>(roles);
        return response;
    }
}
