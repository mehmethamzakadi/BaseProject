using BaseProject.Application.Abstractions;

namespace BaseProject.Infrastructure.Services;

public sealed class CurrentUserService(IExecutionContextAccessor executionContextAccessor) : ICurrentUserService
{
    public Guid? GetCurrentUserId()
    {
        return executionContextAccessor.GetCurrentUserId();
    }
}
