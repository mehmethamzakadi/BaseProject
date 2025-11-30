using AutoMapper;
using BaseProject.Application.Features.ActivityLogs.Queries.GetPaginatedList;
using BaseProject.Domain.Common.Paging;
using BaseProject.Domain.Common.Responses;
using BaseProject.Domain.Entities;

namespace BaseProject.Application.Features.ActivityLogs.Profiles;

public sealed class ActivityLogProfile : Profile
{
    public ActivityLogProfile()
    {
        CreateMap<ActivityLog, GetPaginatedActivityLogsResponse>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty));

        CreateMap<Paginate<ActivityLog>, PaginatedListResponse<GetPaginatedActivityLogsResponse>>();
    }
}
