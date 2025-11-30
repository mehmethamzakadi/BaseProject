using BaseProject.Domain.Common.Results;
using MediatR;

namespace BaseProject.Application.Features.Users.Queries.GetCurrentUserProfile;

public sealed record GetCurrentUserProfileQuery() : IRequest<IDataResult<GetCurrentUserProfileResponse>>;
