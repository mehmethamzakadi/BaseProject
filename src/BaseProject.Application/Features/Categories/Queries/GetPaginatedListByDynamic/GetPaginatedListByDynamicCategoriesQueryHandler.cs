using AutoMapper;
using BaseProject.Application.Abstractions;
using BaseProject.Application.Common.Caching;
using BaseProject.Domain.Common.Paging;
using BaseProject.Domain.Common.Responses;
using BaseProject.Domain.Entities;
using BaseProject.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseProject.Application.Features.Categories.Queries.GetPaginatedListByDynamic;

public sealed class GetPaginatedListByDynamicCategoriesQueryHandler(
    ICategoryRepository categoryRepository,
    IMapper mapper,
    ICacheService cacheService) : IRequestHandler<GetPaginatedListByDynamicCategoriesQuery, PaginatedListResponse<GetPaginatedListByDynamicCategoriesResponse>>
{
    public async Task<PaginatedListResponse<GetPaginatedListByDynamicCategoriesResponse>> Handle(GetPaginatedListByDynamicCategoriesQuery request, CancellationToken cancellationToken)
    {
        var pagination = request.DataGridRequest.PaginatedRequest;
        var versionKey = CacheKeys.CategoryGridVersion();
        var versionToken = await cacheService.Get<string>(versionKey);
        if (string.IsNullOrWhiteSpace(versionToken))
        {
            versionToken = Guid.NewGuid().ToString("N");
            await cacheService.Add(versionKey, versionToken, null, null);
        }

        var cacheKey = CacheKeys.CategoryGrid(versionToken, pagination.PageIndex, pagination.PageSize, request.DataGridRequest.DynamicQuery);
        var cachedResponse = await cacheService.Get<PaginatedListResponse<GetPaginatedListByDynamicCategoriesResponse>>(cacheKey);
        if (cachedResponse is not null)
        {
            return cachedResponse;
        }

        // ✅ Read-only sorgu - tracking'e gerek yok (performans için)
        Paginate<Category> categoriesDynamic = await categoryRepository.GetPaginatedListByDynamicAsync(
            dynamic: request.DataGridRequest.DynamicQuery,
            index: pagination.PageIndex,
            size: pagination.PageSize,
            include: q => q.Include(c => c.Parent),
            enableTracking: false,
            cancellationToken: cancellationToken
        );

        PaginatedListResponse<GetPaginatedListByDynamicCategoriesResponse> response = mapper.Map<PaginatedListResponse<GetPaginatedListByDynamicCategoriesResponse>>(categoriesDynamic);
        await cacheService.Add(
            cacheKey,
            response,
            DateTimeOffset.UtcNow.Add(CacheDurations.CategoryGrid),
            null);

        return response;
    }
}
