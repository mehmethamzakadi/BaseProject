using BaseProject.Domain.Entities;
using BaseProject.Domain.Repositories;
using BaseProject.Persistence.Contexts;

namespace BaseProject.Persistence.Repositories;

public class ImageRepository(BaseProjectDbContext dbContext) : EfRepositoryBase<Image, BaseProjectDbContext>(dbContext), IImageRepository
{
}
