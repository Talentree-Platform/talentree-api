using Microsoft.EntityFrameworkCore;
using System.Linq;
using Talentree.Core.Entities;
using Talentree.Core.Specifications;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Talentree.Repository
{
    /// <summary>
    /// Converts a Specification into an actual EF Core IQueryable
    /// This is the BRIDGE between your business logic and EF Core
    /// </summary>
    /// <remarks>
    /// Order of operations: WHERE → INCLUDE → STRING INCLUDES → ORDER BY → SKIP/TAKE
    /// </remarks>
    internal static class SpecificationsEvaluator<TEntity> where TEntity : class
    {
        /// <summary>
        /// Builds a complete IQueryable by applying all specification criteria
        /// The order of operations matters!
        /// Order: WHERE → INCLUDE → ORDER BY → SKIP/TAKE
        /// </summary>
        /// <param name="inputquery">Base query (usually DbSet<T>)</param>
        /// <param name="specifications">The specification to apply</param>
        /// <returns>Fully configured query ready for execution</returns>
        public static IQueryable<TEntity> CreateQuery(
            IQueryable<TEntity> inputquery,
            ISpecifications<TEntity> specifications)
        {
            var Query = inputquery;  // Start with base query (DbSet)

            // Null check: if no specification, return as-is
            if (specifications is not null)
            {
                // ===============================
                // STEP 1: Apply WHERE clause (Criteria)
                // ===============================
                if (specifications.Criteria is not null)
                {
                    // Translates to: SELECT * FROM Products WHERE [Criteria]
                    Query = Query.Where(specifications.Criteria);

                    // Example:
                    // Input:  DbSet<Product>
                    // Criteria: p => p.Price > 1000
                    // Output: SELECT * FROM Products WHERE Price > 1000
                }
                // ── STEP 2: Expression-based includes (single-level) ──
                if (specifications.Includes?.Any() == true)
                    Query = specifications.Includes.Aggregate(
                        Query,
                        (current, include) => current.Include(include));

                // ── STEP 3: String-based includes (nested levels) ─────
                // Supports dot-notation e.g. "Items.RawMaterial.Supplier"
                // which translates to .Include("Items").ThenInclude("RawMaterial")...
                if (specifications.IncludeStrings?.Any() == true)
                    Query = specifications.IncludeStrings.Aggregate(
                        Query,
                        (current, include) => current.Include(include));

                // ===============================
                // STEP 4: Apply SORTING (ORDER BY)
                // ===============================

                // Ascending order
                if (specifications.OrderBy is not null)
                {
                    Query = Query.OrderBy(specifications.OrderBy);

                    // Example:
                    // Input:  ... WHERE Price > 1000
                    // OrderBy: p => p.Name
                    // Output: ... WHERE Price > 1000 ORDER BY Name ASC
                }

                // Descending order
                if (specifications.OrderByDescending is not null)
                {
                    Query = Query.OrderByDescending(specifications.OrderByDescending);

                    // Example:
                    // Input:  ... WHERE Price > 1000
                    // OrderBy: p => p.Price
                    // Output: ... WHERE Price > 1000 ORDER BY Price DESC
                }

                // -- Note: If both OrderBy and OrderByDescending are set,
                // only OrderByDescending will be applied (last one wins)
                // This is a limitation of the current implementation

                // ===============================
                // STEP 5: Apply PAGINATION (SKIP/TAKE)
                // ===============================
                if (specifications.IsPaginated)
                {
                    Query = Query.Skip(specifications.Skip)
                                 .Take(specifications.Take);

                    // Example:
                    // Skip = 20, Take = 10
                    // SQL: ... OFFSET 20 ROWS FETCH NEXT 10 ROWS ONLY

                    // -- IMPORTANT: Pagination MUST come after OrderBy!
                    // SQL Server requires ORDER BY before OFFSET/FETCH
                }
            }

            return Query;  // Return fully configured query
        }
    }
}