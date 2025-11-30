using BaseProject.Application.Abstractions;
using BaseProject.Domain.Common;
using BaseProject.Domain.Common.Results;
using BaseProject.Domain.Events.PermissionEvents;
using BaseProject.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using IResult = BaseProject.Domain.Common.Results.IResult;

namespace BaseProject.Application.Features.Permissions.Commands.AssignPermissionsToRole;

public class AssignPermissionsToRoleCommandHandler : IRequestHandler<AssignPermissionsToRoleCommand, IResult>
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public AssignPermissionsToRoleCommandHandler(
        IPermissionRepository permissionRepository,
        IRoleRepository roleRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _permissionRepository = permissionRepository;
        _roleRepository = roleRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IResult> Handle(AssignPermissionsToRoleCommand request, CancellationToken cancellationToken)
    {
        // Rol kontrolü
        var role = _roleRepository.GetRoleById(request.RoleId);
        if (role == null)
        {
            return new ErrorResult("Rol bulunamadı");
        }

        // ✅ FIXED: Using repository-specific method instead of Query() leak
        // Permission bilgilerini al (event için)
        var permissionsEntities = await _permissionRepository.GetByIdsAsync(request.PermissionIds, cancellationToken);
        var permissions = permissionsEntities.Select(p => p.Name).ToList();

        // Repository üzerinden permission'ları ata
        await _permissionRepository.AssignPermissionsToRoleAsync(request.RoleId, request.PermissionIds, cancellationToken);

        // Domain event ekle
        role.AddDomainEvent(new PermissionsAssignedToRoleEvent(role.Id, role.Name!, permissions));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SuccessResult("Permission'lar başarıyla atandı");
    }
}