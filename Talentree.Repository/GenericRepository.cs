using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talentree.Core.Entities;
using Talentree.Core.Repository.Contract;
using Talentree.Core.Specifications;
using Talentree.Repository.Data;

namespace Talentree.Repository
{
    /*
==========================================
GenericRepository<T>
==========================================

Responsibilities:
- Acts as a centralized data access layer for all entities
- Encapsulates Entity Framework Core logic
- Works with Specification Pattern for filtering, sorting, includes, and pagination
- Does NOT call SaveChanges (handled by Unit of Work)

Design Decisions:
- Read operations are async and optimized for scalability
- Write operations (Add/Update/Delete) only track changes
- Specifications are evaluated using SpecificationsEvaluator<T>
- Keeps repository thin and reusable

Best Practices:
- Use AsNoTracking() for read-only queries to improve performance
- Pagination and filtering logic must live inside Specifications
- Always access repository through Unit of Work

*/

    internal class GenericRepository<T>(TalentreeDbContext dbContext) : IGenericRepository<T> where T : BaseEntity
    {
        /// <summary>
        /// Retrieves all entities of type T from the database.
        /// Warning: This loads all records into memory - consider using specifications for filtering large datasets.
        /// </summary>
        /// <returns>A collection of all entities</returns>
        // Performance: No tracking needed for read-only operations
        async Task<IEnumerable<T>> IGenericRepository<T>.GetAllAsync() => await dbContext.Set<T>().AsNoTracking().ToListAsync();


        /// <summary>
        /// Retrieves a single entity by its primary key.
        /// </summary>
        /// <param name="id">The primary key value</param>
        /// <returns>The entity if found; otherwise null</returns>
        // FindAsync is optimized for primary key lookups and checks the context cache first
        async Task<T?> IGenericRepository<T>.GetByIdAsync(int id) => await dbContext.Set<T>().FindAsync(id);

        // <summary>
        /// Adds a new entity to the context.
        /// Note: Changes are not persisted until SaveChangesAsync is called on the Unit of Work.
        /// </summary>
        /// <param name="entity">The entity to add</param>
        void IGenericRepository<T>.Add(T entity) => dbContext.Set<T>().Add(entity);

        // <summary>
        /// Marks an entity as modified.
        /// Note: Changes are not persisted until SaveChangesAsync is called on the Unit of Work.
        /// </summary>
        /// <param name="entity">The entity to update</param>
        void IGenericRepository<T>.Update(T entity) => dbContext.Set<T>().Update(entity);

        /// <summary>
        /// Marks an entity for deletion.
        /// Note: Changes are not persisted until SaveChangesAsync is called on the Unit of Work.
        /// </summary>
        /// <param name="entity">The entity to delete</param>
        void IGenericRepository<T>.Delete(T entity) => dbContext.Set<T>().Remove(entity);

        /// <summary>
        /// Retrieves entities that match the given specifications.
        /// Supports filtering, ordering, pagination, and eager loading through the specification pattern.
        /// </summary>
        /// <param name="specifications">The specification defining query criteria</param>
        /// <returns>A collection of entities matching the specifications</returns>
        public async Task<IEnumerable<T>> GetAllWithSpecificationsAsync(ISpecifications<T> specifications)
        {
            return await SpecificationsEvalutor<T>.CreateQuery(dbContext.Set<T>(), specifications).ToListAsync();


        }

        /// <summary>
        /// Retrieves a single entity that matches the given specifications.
        /// Use this when you expect exactly one or no results.
        /// </summary>
        /// <param name="specifications">The specification defining query criteria</param>
        /// <returns>The first entity matching the specifications, or null if none found</returns>
        public async Task<T?> GetByIdWithSpecificationsAsync(ISpecifications<T> specifications)
        {
            return await SpecificationsEvalutor<T>.CreateQuery(dbContext.Set<T>(), specifications).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Counts entities that match the given specifications.
        /// Useful for pagination to get total record count.
        /// </summary>
        /// <param name="specifications">The specification defining query criteria</param>
        /// <returns>The count of matching entities</returns>
        public async Task<int> GetCountWithSpecificationsAsync(ISpecifications<T> specifications)
        {
            var Query = SpecificationsEvalutor<T>.CreateQuery(dbContext.Set<T>(), specifications);
            return await Query.CountAsync();
        }
    }
}
