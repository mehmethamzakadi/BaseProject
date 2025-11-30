using BaseProject.Domain.Common.Results;
using MediatR;

namespace BaseProject.Application.Features.Users.Commands.UpdateCurrentUserProfile;

public sealed record UpdateCurrentUserProfileCommand(
    string UserName,
    string Email,
    string? PhoneNumber,
    string? ProfilePictureUrl) : IRequest<IResult>;
