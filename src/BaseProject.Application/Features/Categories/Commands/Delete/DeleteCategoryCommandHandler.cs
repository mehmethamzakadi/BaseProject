using BaseProject.Application.Abstractions;
using BaseProject.Application.Common.Caching;
using BaseProject.Application.Common.Constants;
using BaseProject.Domain.Common;
using BaseProject.Domain.Common.Results;
using BaseProject.Domain.Repositories;
using MediatR;
using IResult = BaseProject.Domain.Common.Results.IResult;

namespace BaseProject.Application.Features.Categories.Commands.Delete;

/// <summary>
/// Handler for deleting a category
/// </summary>
public sealed class DeleteCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    ICacheService cacheService,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteCategoryCommand, IResult>
{
    public async Task<IResult> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetAsync(predicate: x => x.Id == request.Id, enableTracking: true, cancellationToken: cancellationToken);
        if (category is null)
            return new ErrorResult(ResponseMessages.Category.NotFound);

        // Alt kategori kontrolü - eğer alt kategoriler varsa silinemez
        var hasChildren = await categoryRepository.HasChildrenAsync(request.Id, cancellationToken);
        if (hasChildren)
            return new ErrorResult("Bu kategorinin alt kategorileri bulunmaktadır. Önce alt kategorileri silmeniz gerekmektedir.");

        category.Delete();
        categoryRepository.Delete(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cacheService.Remove(CacheKeys.Category(category.Id));

        await cacheService.Add(
            CacheKeys.CategoryGridVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return new SuccessResult(ResponseMessages.Category.Deleted);
    }
}
