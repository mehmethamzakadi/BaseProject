
using AutoMapper;
using BaseProject.Application.Features.Categories.Commands.Create;
using BaseProject.Application.Features.Categories.Commands.Delete;
using BaseProject.Application.Features.Categories.Commands.Update;
using BaseProject.Application.Features.Categories.Queries.GetAll;
using BaseProject.Application.Features.Categories.Queries.GetById;
using BaseProject.Application.Features.Categories.Queries.GetPaginatedListByDynamic;
using BaseProject.Domain.Common.Paging;
using BaseProject.Domain.Common.Responses;
using BaseProject.Domain.Entities;

namespace BaseProject.Application.Features.Categories.Profiles
{
    public sealed class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            CreateMap<Category, CreateCategoryCommand>().ReverseMap();
            CreateMap<Category, UpdateCategoryCommand>().ReverseMap();
            CreateMap<Category, DeleteCategoryCommand>().ReverseMap();

            CreateMap<Category, GetPaginatedListByDynamicCategoriesResponse>()
                .ForMember(dest => dest.ParentName, opt => opt.MapFrom(src => src.Parent != null ? src.Parent.Name : null))
                .ReverseMap();
            CreateMap<Category, GetAllListCategoriesResponse>().ReverseMap();

            CreateMap<Category, GetByIdCategoryResponse>().ReverseMap();
            CreateMap<Paginate<Category>, PaginatedListResponse<GetPaginatedListByDynamicCategoriesResponse>>().ReverseMap();


        }
    }
}
