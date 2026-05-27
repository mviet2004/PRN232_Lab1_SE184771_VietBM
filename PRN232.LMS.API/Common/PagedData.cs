namespace PRN232.LMS.API.Common;

public class PagedData<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public PaginationMetadata Pagination { get; set; } = new();
}
