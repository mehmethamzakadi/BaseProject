
using BaseProject.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseProject.Persistence.Configurations
{
    /// <summary>
    /// Tüm BaseEntity türevleri için ortak yapılandırma
    /// </summary>
    public class BaseConfiguraiton<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : BaseEntity
    {
        public virtual void Configure(EntityTypeBuilder<TEntity> builder)
        {
            // Primary key
            builder.HasKey(x => x.Id);
            
            // ✅ Concurrency token - Optimistic locking
            builder.Property(x => x.RowVersion)
                .IsRowVersion()
                .HasColumnName("RowVersion");
            
            // ✅ Soft delete filter - BaseProjectDbContext'te reflection ile otomatik uygulanıyor
            // Burada tekrar uygulamaya gerek yok, çünkü BaseProjectDbContext.OnModelCreating'de
            // tüm ISoftDeletable entity'lere otomatik olarak filter uygulanıyor
            // builder.HasQueryFilter(x => !x.IsDeleted); // Yorum satırına alındı - BaseProjectDbContext'te zaten var
        }
    }
}
