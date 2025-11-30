using BaseProject.Domain.Constants;
using BaseProject.Domain.Entities;
using BaseProject.Persistence.Contexts;
using Microsoft.Extensions.Logging;

namespace BaseProject.Persistence.DatabaseInitializer.Seeders;

/// <summary>
/// Permission seed işlemlerini gerçekleştirir
/// Sistemdeki tüm permission'ları oluşturur
/// </summary>
public class PermissionSeeder : BaseSeeder
{
    public PermissionSeeder(BaseProjectDbContext context, ILogger<PermissionSeeder> logger) 
        : base(context, logger)
    {
    }

    public override int Order => 2; // Rollerden sonra permission'lar
    public override string Name => "Permission Seeder";

    protected override async Task SeedDataAsync(CancellationToken cancellationToken)
    {
        var createdDate = new DateTime(2025, 10, 23, 7, 0, 0, DateTimeKind.Utc);
        
        var allPermissionNames = Permissions.GetAllPermissions();
        var permissions = new List<Permission>();

        for (int index = 0; index < allPermissionNames.Count; index++)
        {
            var permissionName = allPermissionNames[index];
            var parts = permissionName.Split('.');
            var module = parts[0];
            var type = parts.Length > 1 ? parts[1] : "Custom";

            var permission = Permission.Create(
                permissionName,
                module,
                type,
                GetPermissionDescription(permissionName)
            );

            // Sabit ID ve tarihleri EF Core ile set et
            var entry = Context.Entry(permission);
            entry.Property("Id").CurrentValue = Guid.Parse($"30000000-0000-0000-0000-000000000{index + 1:D3}");
            entry.Property("CreatedDate").CurrentValue = createdDate;
            entry.Property("IsDeleted").CurrentValue = false;

            permissions.Add(permission);
        }

        await AddRangeIfNotExistsAsync(permissions, p => (Guid)Context.Entry(p).Property("Id").CurrentValue!, cancellationToken);
    }

    private static string GetPermissionDescription(string permissionName)
    {
        return permissionName switch
        {
            Permissions.DashboardView => "Admin paneli dashboard'una erişim yetkisi",
            Permissions.UsersCreate => "Yeni kullanıcı oluşturma yetkisi",
            Permissions.UsersRead => "Kullanıcı bilgilerini görüntüleme yetkisi",
            Permissions.UsersUpdate => "Kullanıcı bilgilerini güncelleme yetkisi",
            Permissions.UsersDelete => "Kullanıcı silme yetkisi",
            Permissions.UsersViewAll => "Tüm kullanıcıları görüntüleme yetkisi",
            Permissions.RolesCreate => "Yeni rol oluşturma yetkisi",
            Permissions.RolesRead => "Rol bilgilerini görüntüleme yetkisi",
            Permissions.RolesUpdate => "Rol bilgilerini güncelleme yetkisi",
            Permissions.RolesDelete => "Rol silme yetkisi",
            Permissions.RolesViewAll => "Tüm rolleri görüntüleme yetkisi",
            Permissions.RolesAssignPermissions => "Role yetki atama yetkisi",
            Permissions.CategoriesCreate => "Yeni kategori oluşturma yetkisi",
            Permissions.CategoriesRead => "Kategori görüntüleme yetkisi",
            Permissions.CategoriesUpdate => "Kategori güncelleme yetkisi",
            Permissions.CategoriesDelete => "Kategori silme yetkisi",
            Permissions.CategoriesViewAll => "Tüm kategorileri görüntüleme yetkisi",
            Permissions.MediaUpload => "Medya dosyası yükleme yetkisi",
            Permissions.ActivityLogsView => "Aktivite loglarını görüntüleme yetkisi",
            _ => $"{permissionName} permission"
        };
    }
}
