using BaseProject.Domain.Common.Requests;
using BaseProject.Domain.Common.Responses;
using MediatR;

namespace BaseProject.Application.Features.ActivityLogs.Queries.GetPaginatedList;

public class GetPaginatedActivityLogsQuery : IRequest<PaginatedListResponse<GetPaginatedActivityLogsResponse>>
{
    public DataGridRequest Request { get; set; }

    public GetPaginatedActivityLogsQuery(DataGridRequest request)
    {
        Request = request;
    }
}
