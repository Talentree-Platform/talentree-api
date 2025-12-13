using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talentree.Core.Entities;
using Talentree.Core.Specifications;

namespace Talentree.Repository
{
    /// <summary>
    /// Evaluates and applies specification criteria to build the final EF Core query.
    /// This is the bridge between the specification pattern and Entity Framework Core.
    /// </summary>
    /// <typeparam name="TEntity">The entity type that inherits from BaseEntity</typeparam>

    internal static class SpecificationsEvalutor<TEntity> where TEntity : BaseEntity
    {
        
        /// <summary>
        /// Builds an IQueryable by applying all specification criteria to the input query.
        /// The order of operations is: Where -> Include -> OrderBy -> Pagination
        /// </summary>
        /// <param name="inputQuery">The base queryable to start from (usually DbSet)</param>
        /// <param name="specifications">The specification containing query criteria</param>
        /// <returns>A fully configured IQueryable ready for execution</returns>
        public static IQueryable<TEntity> CreateQuery (IQueryable<TEntity> inputquery , ISpecifications<TEntity> specifications)
        {
            var Query = inputquery;
            if (specifications is not null)
            {
                #region Adding Criteria To The Query
                if (specifications.Criteria is not null)
                {
                    Query = Query.Where(specifications.Criteria);
                } 
                #endregion

                #region Adding Includes For the Query
                if (specifications.Includes != null && specifications.Includes.Any())
                {
                    Query = specifications.Includes.Aggregate(Query, (current, includeExp) => current.Include(includeExp));
                }
                #endregion

                #region Adding Sorting For the Query
                if (specifications.OrderBy is not null)
                {
                    Query = Query.OrderBy(specifications.OrderBy);
                }
                if (specifications.OrderByDescending is not null)
                {
                    Query = Query.OrderByDescending(specifications.OrderByDescending);
                }
                #endregion

                #region Adding Pagination to the Query
                if (specifications.IsPaginated)
                {
                    Query = Query.Skip(specifications.Skip).Take(specifications.Take);
                } 
                #endregion

            }

            return Query;
        }
    }
}
