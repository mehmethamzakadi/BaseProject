using BaseProject.Domain.Common;
using BaseProject.Domain.Common.Results;
using BaseProject.Domain.Constants;
using BaseProject.Domain.Entities;
using BaseProject.Domain.Repositories;
using BaseProject.Domain.Services;
using MediatR;

namespace BaseProject.Application.Features.Auths.Register;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, IResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserDomainService _userDomainService;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IUserDomainService userDomainService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _userDomainService = userDomainService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        User? existingUser = await _userRepository.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            return new ErrorResult("Bu e-posta adresi zaten kullanılıyor!");
        }

        var user = User.Create(request.UserName, request.Email, string.Empty);

        var passwordResult = _userDomainService.SetPassword(user, request.Password);
        if (!passwordResult.Success)
            return passwordResult;

        await _userRepository.AddAsync(user);

        var userRole = await _roleRepository.GetAsync(r => r.NormalizedName == UserRoles.User.ToUpperInvariant());
        if (userRole != null)
        {
            var roleResult = _userDomainService.AddToRole(user, userRole);
            if (!roleResult.Success)
                return roleResult;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new SuccessResult("Kayıt işlemi başarılı. Giriş yapabilirsiniz.");
    }
}
