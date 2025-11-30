using BaseProject.Domain.Common.Dynamic;

namespace BaseProject.Domain.Common.Requests;

public class DataGridRequest
{
    public PaginatedRequest PaginatedRequest { get; set; } = new();
    public DynamicQuery? DynamicQuery { get; set; }

    public DataGridRequest()
    {
    }

    public DataGridRequest(PaginatedRequest paginatedRequest, DynamicQuery? dynamicQuery)
    {
        PaginatedRequest = paginatedRequest ?? new PaginatedRequest();
        DynamicQuery = dynamicQuery;
    }
}
