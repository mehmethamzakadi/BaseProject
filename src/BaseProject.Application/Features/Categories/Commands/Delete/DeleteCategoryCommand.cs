using BaseProject.Application.Behaviors;
using BaseProject.Application.Common.Caching;
using BaseProject.Domain.Common.Results;
using MediatR;

namespace BaseProject.Application.Features.Categories.Commands.Delete;

public sealed record DeleteCategoryCommand(Guid Id) : IRequest<IResult>, IInvalidateCache
{
    public IEnumerable<string> GetCacheKeysToInvalidate()
    {
        yield return CacheKeys.Category(Id);
        yield return CacheKeys.CategoryGridVersion();
    }
}
