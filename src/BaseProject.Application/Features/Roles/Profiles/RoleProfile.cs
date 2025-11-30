using AutoMapper;
using BaseProject.Application.Features.Roles.Queries.GetList;
using BaseProject.Application.Features.Roles.Queries.GetRoleById;
using BaseProject.Domain.Common.Paging;
using BaseProject.Domain.Common.Responses;
using BaseProject.Domain.Entities;

namespace BaseProject.Application.Features.Roles.Profiles
{
    public class RoleProfile : Profile
    {
        public RoleProfile()
        {
            CreateMap<Role, GetListRoleResponse>().ReverseMap();
            CreateMap<Role, GetRoleByIdQueryResponse>().ReverseMap();

            CreateMap<Paginate<Role>, PaginatedListResponse<GetListRoleResponse>>().ReverseMap();
        }
    }
}
