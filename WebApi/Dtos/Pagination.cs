using System.ComponentModel.DataAnnotations;

namespace WebApi.Dtos
{
    public class PaginationQuery
    {
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;
    }

    public class OrderQuery : PaginationQuery
    {
        public int? CustomerId { get; set; }

        public bool? IsPaid { get; set; }

        public decimal? MinAmount { get; set; }
    }

    public class PagedResult<T>
    {
        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public int TotalPages { get; set; }

        public IReadOnlyList<T> Items { get; set; } = [];
    }
}
