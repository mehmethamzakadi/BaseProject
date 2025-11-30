using BaseProject.Application.Features.ActivityLogs.Queries.GetPaginatedList;
using BaseProject.Domain.Common.Requests;
using BaseProject.Domain.Common.Responses;
using BaseProject.Domain.Constants;
using BaseProject.Infrastructure.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BaseProject.API.Controllers;

public class ActivityLogsController(IMediator mediator) : BaseApiController(mediator)
{
    /// <summary>
    /// Activity log'ları paginated ve filtrelenmiş şekilde getirir
    /// </summary>
    [HttpPost("search")]
    [HasPermission(Permissions.ActivityLogsView)]
    public async Task<IActionResult> GetPaginatedList([FromBody] DataGridRequest request)
    {
        PaginatedListResponse<GetPaginatedActivityLogsResponse> response =
            await Mediator.Send(new GetPaginatedActivityLogsQuery(request));
        return Ok(response);
    }
}
