using System.Linq;

namespace BaseProject.Domain.Common;

public interface IQuery<T>
{
    IQueryable<T> Query();
}
