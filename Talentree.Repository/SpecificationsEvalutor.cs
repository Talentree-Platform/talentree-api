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
    internal static class SpecificationsEvalutor<TEntity> where TEntity : BaseEntity
    {
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
