using Microsoft.AspNetCore.Authorization;

namespace BaseProject.Infrastructure.Authorization;

/// <summary>
/// Permission tabanlı authorize attribute.
/// Controller veya action'lara belirli permission gerektiren yetkilendirme ekler.
/// </summary>
public class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission)
    {
        Policy = permission;
    }
}
