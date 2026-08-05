namespace SmartCourt.Common.Models;

public class PagedResponse<T> : ApiResponse<T>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalRecords { get; set; }
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;

    public static PagedResponse<T> OkPaged(T data, int pageNumber, int pageSize, int totalRecords, int totalPages, string? message = null)
    {
        return new PagedResponse<T>
        {
            Success = true,
            Data = data,
            StatusCode = 200,
            Message = message,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        };
    }
}
