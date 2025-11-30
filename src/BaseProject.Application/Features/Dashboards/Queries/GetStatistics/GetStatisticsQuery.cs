using MediatR;

namespace BaseProject.Application.Features.Dashboards.Queries.GetStatistics;

public sealed record GetStatisticsQuery : IRequest<GetStatisticsResponse>;
