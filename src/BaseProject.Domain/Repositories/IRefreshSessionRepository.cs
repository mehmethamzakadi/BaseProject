using BaseProject.Domain.Common;
using BaseProject.Domain.Entities;

namespace BaseProject.Domain.Repositories;

public interface IRefreshSessionRepository : IRepository<RefreshSession>
{
    Task<RefreshSession?> GetByTokenHashAsync(string tokenHash, bool includeDeleted = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RefreshSession>> GetActiveSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> DeleteExpiredSessionsAsync(CancellationToken cancellationToken = default);
}
