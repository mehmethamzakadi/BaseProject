using AutoMapper;
using BaseProject.Domain.Common.Results;
using BaseProject.Domain.Entities;
using BaseProject.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseProject.Application.Features.Users.Queries.GetById;

public sealed class GetByIdUserQueryHandler(
    IUserRepository userRepository,
    IMapper mapper) : IRequestHandler<GetByIdUserQuery, IDataResult<GetByIdUserResponse>>
{
    public async Task<IDataResult<GetByIdUserResponse>> Handle(GetByIdUserQuery request, CancellationToken cancellationToken)
    {
        // ✅ Read-only sorgu - tracking'e gerek yok (performans için)
        User? user = await userRepository.GetAsync(
            u => u.Id == request.Id,
            include: q => q.Include(u => u.UserRoles).ThenInclude(ur => ur.Role),
            enableTracking: false,
            cancellationToken: cancellationToken);

        if (user is null)
        {
            return new ErrorDataResult<GetByIdUserResponse>("Kullanıcı bulunamadı!");
        }

        GetByIdUserResponse userResponse = mapper.Map<GetByIdUserResponse>(user);
        return new SuccessDataResult<GetByIdUserResponse>(userResponse);
    }
}
