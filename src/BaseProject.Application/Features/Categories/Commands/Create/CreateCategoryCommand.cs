using BaseProject.Application.Behaviors;
using BaseProject.Application.Common.Caching;
using BaseProject.Domain.Common.Results;
using MediatR;

namespace BaseProject.Application.Features.Categories.Commands.Create;

public sealed record CreateCategoryCommand(string Name, string? Description = null, Guid? ParentId = null) : IRequest<IResult>, IInvalidateCache
{
    public IEnumerable<string> GetCacheKeysToInvalidate()
    {
        yield return CacheKeys.CategoryGridVersion();
    }
}
