using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Talentree.Core.Entities;

namespace Talentree.Core.Specifications
{
    /// <summary>
    /// Base implementation of the Specification pattern providing fluent API for building queries.
    /// Inherit from this class to create specific query specifications for your entities.
    /// </summary>
    /// <typeparam name="T">The entity type that inherits from BaseEntity</typeparam>
    /// <example>
    /// public class ActiveUsersSpec : BaseSpecifications<User>
    /// {
    ///     public ActiveUsersSpec() : base(u => u.IsActive)
    ///     {
    ///         AddInclude(u => u.Profile);
    ///         AddOrderBy(u => u.Name);
    ///         ApplyPagination(1, 10);
    ///     }
    /// }
    /// </example>
    public class BaseSpecifications<T> : ISpecifications<T> where T : BaseEntity
    {
        #region Criteria
        public Expression<Func<T, bool>> Criteria { get; private set; } = null;

        // For specifications WITH criteria
        protected BaseSpecifications(Expression<Func<T, bool>> _Criteria)
        {
            Criteria = _Criteria;
        }
        // For specifications WITHOUT criteria
        protected BaseSpecifications()
        {
        }
        #endregion

        #region Includes
        //Includes property and Method implementation  
        public ICollection<Expression<Func<T, object>>> Includes { get; private set; } = [];

        protected void AddInclude(Expression<Func<T, object>> includeExpression)
        {
            Includes.Add(includeExpression);
        }
        #endregion

        #region Sorting
        //Sorting 
        public Expression<Func<T, object>> OrderBy { get; private set; }

        public Expression<Func<T, object>> OrderByDescending { get; private set; }

        protected void AddOrderBy(Expression<Func<T, object>> orderByExpression)
        {
            OrderBy = orderByExpression;
        }
        protected void AddOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
        {
            OrderByDescending = orderByDescExpression;
        }
        #endregion
        #region Pagination
        //Pagination
        public int Take { get; private set; }

        public int Skip { get; private set; }

        public bool IsPaginated { get; private set; }

        public void ApplyPagination(int PageIndex, int PageSize)
        {
            IsPaginated = true;
            Take = PageSize;
            Skip = (PageIndex - 1) * PageSize;
        } 
        #endregion
    }
}
