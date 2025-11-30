using BaseProject.Domain.Common;
using BaseProject.Domain.Common.Results;
using BaseProject.Domain.Entities;
using BaseProject.Domain.Repositories;
using MediatR;
using IResult = BlogApp.Domain.Common.Results.IResult;

namespace BaseProject.Application.Features.Roles.Commands.Create;

public sealed class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, IResult>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRoleCommandHandler(
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IResult> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var checkRole = _roleRepository.AnyRole(request.Name);
        if (checkRole)
            return new ErrorResult("Eklemek istediğiniz Rol sistemde mevcut!");

        var role = Role.Create(request.Name);
        var result = await _roleRepository.CreateRole(role);
        if (!result.Success)
            return new ErrorResult("İşlem sırasında hata oluştu!");

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SuccessResult("Rol oluşturuldu.");
    }
}
