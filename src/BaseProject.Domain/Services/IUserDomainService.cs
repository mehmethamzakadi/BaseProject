using BaseProject.Domain.Common.Results;
using BaseProject.Domain.Entities;

namespace BaseProject.Domain.Services;

public interface IUserDomainService
{
    IResult SetPassword(User user, string password);
    IResult ResetPassword(User user, string resetToken, string newPassword);
    bool VerifyPassword(User user, string password);
    IResult AddToRole(User user, Role role);
    IResult AddToRoles(User user, IEnumerable<Role> roles);
    IResult RemoveFromRoles(User user, IEnumerable<Role> roles);
}
