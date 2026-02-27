using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Talentree.Core.Entities;

namespace Talentree.Core.Specifications
{
    /// <summary>
    /// Base implementation of the Specification pattern providing fluent API for building queries
    /// All concrete specifications should inherit from this class
    /// </summary>
    /// <typeparam name="T">Entity type that inherits from BaseEntity</typeparam>
    public class BaseSpecifications<T> : ISpecifications<T> where T : class
    {
        #region Criteria (WHERE clause)

        /// <summary>
        /// The WHERE condition for filtering entities
        /// </summary>
        public Expression<Func<T, bool>> Criteria { get; private set; } = null;

        /// <summary>
        /// Constructor for specifications WITH filtering criteria
        /// </summary>
        /// <param name="_Criteria">The filter expression (e.g., p => p.Price > 100)</param>
        protected BaseSpecifications(Expression<Func<T, bool>> _Criteria)
        {
            Criteria = _Criteria;
        }

        /// <summary>
        /// Constructor for specifications WITHOUT filtering criteria (get all)
        /// </summary>
        protected BaseSpecifications()
        {
        }

        #endregion

        #region Includes (Navigation Properties)

        /// <summary>Expression-based includes for single-level navigation properties</summary>
        public ICollection<Expression<Func<T, object>>> Includes { get; private set; } = new List<Expression<Func<T, object>>>();

        /// <summary>
        /// String-based includes for nested navigation properties.
        /// Use this when you need ThenInclude behaviour e.g. "Items.RawMaterial.Supplier"
        /// </summary>
        public ICollection<string> IncludeStrings { get; private set; } = new List<string>();

        /// <summary>Adds a single-level navigation property to be eagerly loaded</summary>
        protected void AddInclude(Expression<Func<T, object>> includeExpression)
        {
            Includes.Add(includeExpression);
        }

        /// <summary>
        /// Adds a nested navigation property path to be eagerly loaded.
        /// Supports dot-notation for multiple levels e.g. "Items.RawMaterial.Supplier"
        /// </summary>
        protected void AddInclude(string includeString)
        {
            IncludeStrings.Add(includeString);
        }

        #endregion

        #region Sorting (ORDER BY)

        /// <summary>
        /// Expression for ascending sort order
        /// </summary>
        public Expression<Func<T, object>> OrderBy { get; private set; }

        /// <summary>
        /// Expression for descending sort order
        /// </summary>
        public Expression<Func<T, object>> OrderByDescending { get; private set; }

        /// <summary>
        /// Sets ascending sort order
        /// </summary>
        /// <param name="orderByExpression">Sort expression</param>
        protected void AddOrderBy(Expression<Func<T, object>> orderByExpression)
        {
            OrderBy = orderByExpression;
        }

        /// <summary>
        /// Sets descending sort order
        /// </summary>
        /// <param name="orderByDescExpression">Sort expression</param>
        protected void AddOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
        {
            OrderByDescending = orderByDescExpression;
        }

        #endregion

        #region Pagination (SKIP/TAKE)

        /// <summary>
        /// Number of records to take (page size)
        /// </summary>
        public int Take { get; private set; }

        /// <summary>
        /// Number of records to skip (offset)
        /// </summary>
        public int Skip { get; private set; }

        /// <summary>
        /// Indicates if pagination is enabled
        /// </summary>
        public bool IsPaginated { get; private set; }

        /// <summary>
        /// Enables pagination and calculates skip/take values
        /// </summary>
        /// <param name="pageIndex">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        protected void ApplyPagination(int pageIndex, int pageSize)
        //^^^^^^^^ Changed from public to protected
        {
            IsPaginated = true;
            Take = pageSize;
            Skip = (pageIndex - 1) * pageSize;
        }

        #endregion
    }
}