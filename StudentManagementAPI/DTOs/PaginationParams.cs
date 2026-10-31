using System;
using System.Collections.Generic;

namespace StudentManagementAPI.DTOs
{
    /// <summary>
    /// Parameters for pagination, searching, filtering and sorting
    /// </summary>
    public class PaginationParams
    {
        private const int MaxPageSize = 50;
        private int _pageSize = 10;

        public int PageNumber { get; set; } = 1;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : (value < 1 ? 10 : value);
        }

        public string? SearchTerm { get; set; }
        public string? Department { get; set; }
        public int? Year { get; set; }
        public string? SortBy { get; set; } // Name, RegisterNumber, Year, CGPA, CreatedAt
        public bool IsDescending { get; set; } = false;
    }

    /// <summary>
    /// Paginated response result wrapper
    /// </summary>
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / (PageSize > 0 ? PageSize : 10));
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}

