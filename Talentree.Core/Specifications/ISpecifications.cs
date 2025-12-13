using System.Linq.Expressions;
using Talentree.Core.Entities;
namespace Talentree.Core.Specifications
{
    /// <summary>
    /// Defines the contract for building database query specifications
    /// Encapsulates query logic including filtering, includes, sorting, and pagination
    /// </summary>
    /// <typeparam name="T">Entity type that inherits from BaseEntity</typeparam>
    public interface ISpecifications<T> where T : BaseEntity
    {
        /// <summary>
        /// WHERE clause criteria (e.g., p => p.Price > 100)
        /// </summary>
        Expression<Func<T, bool>> Criteria { get; }

        /// <summary>
        /// Navigation properties to include (eager loading)
        /// </summary>
        ICollection<Expression<Func<T, object>>> Includes { get; }

        /// <summary>
        /// Ascending sort expression (ORDER BY)
        /// </summary>
        Expression<Func<T, object>> OrderBy { get; }

        /// <summary>
        /// Descending sort expression (ORDER BY DESC)
        /// </summary>
        Expression<Func<T, object>> OrderByDescending { get; }

        /// <summary>
        /// Number of records to take (page size)
        /// </summary>
        int Take { get; }

        /// <summary>
        /// Number of records to skip (offset)
        /// </summary>
        int Skip { get; }

        /// <summary>
        /// Indicates if pagination is enabled
        /// </summary>
        bool IsPaginated { get; }
    }
}