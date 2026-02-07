using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talentree.Service.DTOs
{

    public class Pagination<T> where T : class
    {
        /// <summary>
        /// Current page number (1-based)
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of items across all pages
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Items in current page
        /// </summary>
        public IReadOnlyList<T> Data { get; set; }

        // ============= COMPUTED PROPERTIES =============

        /// <summary>
        /// Total number of pages
        /// </summary>
        public int TotalPages => (int)Math.Ceiling(Count / (double)PageSize);

        public bool HasPrevious => PageIndex > 1;

        public bool HasNext => PageIndex < TotalPages;

        /// <summary>
        /// Index of first item on current page (1-based)
        /// </summary>
        public int FirstItemIndex => Count == 0 ? 0 : (PageIndex - 1) * PageSize + 1;

        /// <summary>
        /// Index of last item on current page (1-based)
        /// </summary>
        public int LastItemIndex => Math.Min(PageIndex * PageSize, Count);

        // ============= CONSTRUCTORS =============

        /// <summary>
        /// Default constructor
        /// </summary>
        public Pagination()
        {
            Data = new List<T>();
        }

        /// <summary>
        /// Create pagination with data
        /// </summary>
        public Pagination(int pageIndex, int pageSize, int count, IReadOnlyList<T> data)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            Count = count;
            Data = data;
        }

        /// <summary>
        /// Create pagination from list
        /// </summary>
        public Pagination(int pageIndex, int pageSize, int count, List<T> data)
            : this(pageIndex, pageSize, count, data.AsReadOnly())
        {
        }

        // ============= FACTORY METHODS =============

        /// <summary>
        /// Create empty pagination result
        /// </summary>
        public static Pagination<T> Empty(int pageIndex = 1, int pageSize = 20)
        {
            return new Pagination<T>(pageIndex, pageSize, 0, new List<T>());
        }

        /// <summary>
        /// Create pagination from existing data
        /// </summary>
        public static Pagination<T> Create(
            IReadOnlyList<T> data,
            int pageIndex,
            int pageSize,
            int totalCount)
        {
            return new Pagination<T>(pageIndex, pageSize, totalCount, data);
        }
    }
}
