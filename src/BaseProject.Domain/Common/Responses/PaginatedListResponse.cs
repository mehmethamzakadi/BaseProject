using BaseProject.Domain.Common.Paging;

namespace BaseProject.Domain.Common.Responses;

public class PaginatedListResponse<T> : BasePageableModel
{
    private IList<T> _items = new List<T>();

    public IList<T> Items
    {
        get => _items;
        set => _items = value ?? new List<T>();
    }
}
