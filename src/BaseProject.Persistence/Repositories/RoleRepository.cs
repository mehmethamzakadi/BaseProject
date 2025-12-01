using BaseProject.Domain.Common.Paging;
using BaseProject.Domain.Common.Results;
using BaseProject.Domain.Entities;
using BaseProject.Domain.Repositories;
using BaseProject.Persistence.Contexts;
using BaseProject.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BaseProject.Persistence.Repositories;

public sealed class RoleRepository(BaseProjectDbContext context) : EfRepositoryBase<Role, BaseProjectDbContext>(context), IRoleRepository
{

    public async Task<Paginate<Role>> GetRoles(int index, int size, CancellationToken cancellationToken)
    {
        // ✅ Read-only sorgu - tracking'e gerek yok (performans için)
        return await Context.Roles
            .AsNoTracking()
            .ToPaginateAsync(index, size, cancellationToken);
    }

    public Role? GetRoleById(Guid id)
    {
        // ⚠️ DEPRECATED: Bu metod artık kullanılmamalı
        // Query handler'larda GetAsync(enableTracking: false) kullanılmalı
        // Command handler'larda GetAsync(enableTracking: true) kullanılmalı
        // Tracking açık tutuluyor çünkü bazı eski kodlar hala kullanıyor olabilir
        var result = Context.Roles
            .FirstOrDefault(x => x.Id == id);
        return result;
    }

    public async Task<IResult> CreateRole(Role role)
    {
        try
        {
            await Context.Roles.AddAsync(role);
            // ✅ REMOVED: SaveChanges - UnitOfWork is responsible for transaction management
            return new SuccessResult("Rol başarıyla oluşturuldu.");
        }
        catch (Exception ex)
        {
            return new ErrorResult($"Rol oluşturulurken hata oluştu: {ex.Message}");
        }
    }

    public Task<IResult> DeleteRole(Role role)
    {
        try
        {
            Context.Roles.Remove(role);
            // ✅ REMOVED: SaveChanges - UnitOfWork is responsible for transaction management
            return Task.FromResult<IResult>(new SuccessResult("Rol başarıyla silindi."));
        }
        catch (Exception ex)
        {
            return Task.FromResult<IResult>(new ErrorResult($"Rol silinirken hata oluştu: {ex.Message}"));
        }
    }

    public Task<IResult> UpdateRole(Role role)
    {
        try
        {
            Context.Roles.Update(role);
            // ✅ REMOVED: SaveChanges - UnitOfWork is responsible for transaction management
            return Task.FromResult<IResult>(new SuccessResult("Rol başarıyla güncellendi."));
        }
        catch (Exception ex)
        {
            return Task.FromResult<IResult>(new ErrorResult($"Rol güncellenirken hata oluştu: {ex.Message}"));
        }
    }

    public async Task<Role?> FindByNameAsync(string roleName)
    {
        // ✅ Validation için kullanılıyor - tracking'e gerek yok (performans için)
        // ✅ NormalizedName üzerinden case-insensitive karşılaştırma
        var normalizedName = roleName.ToUpperInvariant();
        return await Context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.NormalizedName == normalizedName);
    }

    public bool AnyRole(string name)
    {
        // ✅ NormalizedName üzerinden case-insensitive karşılaştırma
        var normalizedName = name.ToUpperInvariant();
        var result = Context.Roles
            .Any(x => x.NormalizedName == normalizedName);

        return result;
    }

    public async Task<List<Role>> GetByIdsAsync(List<Guid> roleIds, CancellationToken cancellationToken = default)
    {
        // ✅ Read-only sorgu - tracking'e gerek yok (performans için)
        return await Query()
            .AsNoTracking()
            .Where(r => roleIds.Contains(r.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await Query().CountAsync(cancellationToken);
    }
}
