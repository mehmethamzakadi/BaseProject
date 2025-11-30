using MediatR;

namespace BaseProject.Application.Features.Dashboards.Queries.GetAiInsights;

public sealed record GetAiInsightsQuery : IRequest<GetAiInsightsResponse>;
