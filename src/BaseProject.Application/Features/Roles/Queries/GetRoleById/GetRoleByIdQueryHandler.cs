using BaseProject.Domain.Common.Results;
using BaseProject.Domain.Entities;
using BaseProject.Domain.Repositories;
using MediatR;

namespace BaseProject.Application.Features.Roles.Queries.GetRoleById;

public sealed class GetRoleByIdQueryHandler(IRoleRepository roleRepository) : IRequestHandler<GetRoleByIdRequest, IDataResult<GetRoleByIdQueryResponse>>
{
    public Task<IDataResult<GetRoleByIdQueryResponse>> Handle(GetRoleByIdRequest request, CancellationToken cancellationToken)
    {
        Role? role = roleRepository.GetRoleById(request.Id);
        if (role is null)
        {
            IDataResult<GetRoleByIdQueryResponse> errorResult = new ErrorDataResult<GetRoleByIdQueryResponse>("Rol bulunamadı!");
            return Task.FromResult(errorResult);
        }

        GetRoleByIdQueryResponse result = new(Id: role.Id, Name: role.Name!);
        IDataResult<GetRoleByIdQueryResponse> successResult = new SuccessDataResult<GetRoleByIdQueryResponse>(result);
        return Task.FromResult(successResult);
    }
}
