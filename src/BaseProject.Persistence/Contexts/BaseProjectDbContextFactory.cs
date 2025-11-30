using BaseProject.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BaseProject.Persistence.Contexts;

/// <summary>
/// Design-time factory for BaseProjectDbContext - used by EF Core migrations
/// </summary>
public class BaseProjectDbContextFactory : IDesignTimeDbContextFactory<BaseProjectDbContext>
{
    public BaseProjectDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BaseProjectDbContext>();

        // PostgreSQL connection string for migrations
        optionsBuilder.UseNpgsql(
            "Host=localhost;Database=BaseProjectDb;Username=postgres;Password=postgres",
            b => b.MigrationsHistoryTable("__EFMigrationsHistory", "public")
        );

        // Use a lightweight execution context accessor for design-time operations
        IExecutionContextAccessor executionContextAccessor = new DesignTimeExecutionContextAccessor();

        return new BaseProjectDbContext(optionsBuilder.Options, executionContextAccessor);
    }

    private sealed class DesignTimeExecutionContextAccessor : IExecutionContextAccessor
    {
        public Guid? GetCurrentUserId() => null;

        public IDisposable BeginScope(Guid userId) => NullScope.Instance;

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
    }
}
