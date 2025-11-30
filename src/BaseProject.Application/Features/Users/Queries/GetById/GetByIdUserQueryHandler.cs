using AutoMapper;
using BaseProject.Domain.Common.Results;
using BaseProject.Domain.Entities;
using BaseProject.Domain.Repositories;
using MediatR;

namespace BaseProject.Application.Features.Users.Queries.GetById;

public sealed class GetByIdUserQueryHandler(
    IUserRepository userRepository,
    IMapper mapper) : IRequestHandler<GetByIdUserQuery, IDataResult<GetByIdUserResponse>>
{
    public async Task<IDataResult<GetByIdUserResponse>> Handle(GetByIdUserQuery request, CancellationToken cancellationToken)
    {
        User? user = await userRepository.FindByIdAsync(request.Id);
        if (user is null)
        {
            return new ErrorDataResult<GetByIdUserResponse>("Kullanıcı bulunamadı!");
        }

        GetByIdUserResponse userResponse = mapper.Map<GetByIdUserResponse>(user);
        return new SuccessDataResult<GetByIdUserResponse>(userResponse);
    }
}
