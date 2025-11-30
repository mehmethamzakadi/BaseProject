using BaseProject.Application.Abstractions;
using BaseProject.Domain.Common.Results;
using BaseProject.Domain.Repositories;
using MediatR;

namespace BaseProject.Application.Features.Users.Queries.GetCurrentUserProfile;

public sealed class GetCurrentUserProfileQueryHandler(
    ICurrentUserService currentUserService,
    IUserRepository userRepository) : IRequestHandler<GetCurrentUserProfileQuery, IDataResult<GetCurrentUserProfileResponse>>
{
    public async Task<IDataResult<GetCurrentUserProfileResponse>> Handle(GetCurrentUserProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();
        if (userId == null)
        {
            return new ErrorDataResult<GetCurrentUserProfileResponse>("Kullanıcı kimliği bulunamadı.");
        }

        var user = await userRepository.FindByIdAsync(userId.Value);
        if (user == null)
        {
            return new ErrorDataResult<GetCurrentUserProfileResponse>("Kullanıcı bulunamadı.");
        }

        var response = new GetCurrentUserProfileResponse(
            user.Id,
            user.UserName,
            user.Email,
            user.PhoneNumber,
            user.ProfilePictureUrl,
            user.EmailConfirmed,
            user.CreatedDate);

        return new SuccessDataResult<GetCurrentUserProfileResponse>(response);
    }
}
