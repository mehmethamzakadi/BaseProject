using BaseProject.Application.Features.Dashboards.Queries.GetAiInsights;
using BaseProject.Application.Features.Dashboards.Queries.GetRecentActivities;
using BaseProject.Application.Features.Dashboards.Queries.GetStatistics;
using BaseProject.Domain.Constants;
using BaseProject.Infrastructure.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BaseProject.API.Controllers
{
    [Route("api/[controller]")]
    public class DashboardsController(IMediator mediator) : BaseApiController(mediator)
    {
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            GetStatisticsResponse response = await Mediator.Send(new GetStatisticsQuery());
            return Ok(response);
        }

        [HttpGet("activities")]
        public async Task<IActionResult> GetRecentActivities([FromQuery] int count = 10)
        {
            GetRecentActivitiesResponse response = await Mediator.Send(new GetRecentActivitiesQuery(count));
            return Ok(response);
        }

        [HttpGet("ai-insights")]
        [HasPermission(BaseProject.Domain.Constants.Permissions.DashboardAIInsights)]
        public async Task<IActionResult> GetAiInsights()
        {
            GetAiInsightsResponse response = await Mediator.Send(new GetAiInsightsQuery());
            return Ok(response);
        }
    }
}
