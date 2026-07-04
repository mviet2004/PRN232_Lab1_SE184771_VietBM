namespace PRN232.LMS.API.Common;

public class PagedData<T>
{
    public List<T> Items { get; set; } = new();
    public PaginationMetadata Pagination { get; set; } = new();
}
