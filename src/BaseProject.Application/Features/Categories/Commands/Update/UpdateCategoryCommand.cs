using BaseProject.Application.Behaviors;
using BaseProject.Application.Common.Caching;
using BaseProject.Domain.Common.Results;
using MediatR;

namespace BaseProject.Application.Features.Categories.Commands.Update;

public sealed record UpdateCategoryCommand(Guid Id, string Name, string? Description = null, Guid? ParentId = null) : IRequest<IResult>, IInvalidateCache
{
    public IEnumerable<string> GetCacheKeysToInvalidate()
    {
        yield return CacheKeys.Category(Id);
        yield return CacheKeys.CategoryGridVersion();
    }
}
