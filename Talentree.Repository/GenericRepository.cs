using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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
    - Centralized data access layer for all entities
    - Encapsulates Entity Framework Core logic
    - Implements Specification Pattern for complex queries
    - Does NOT call SaveChanges (handled by Unit of Work)
    
    Design Decisions:
    - Read operations use AsNoTracking() for better performance
    - Write operations only track changes (no immediate persistence)
    - Specifications evaluated through SpecificationsEvaluator<T>
    - Returns IReadOnlyList to prevent multiple enumeration
    
    Best Practices:
    - Always access through Unit of Work
    - Use specifications for filtering, sorting, pagination
    - Avoid GetAllAsync() for large datasets
    */

    /// <summary>
    /// Generic repository implementation providing data access for entities
    /// </summary>
    /// <typeparam name="T">Entity type inheriting from BaseEntity</typeparam>
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly TalentreeDbContext _dbContext;

        public GenericRepository(TalentreeDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region Basic CRUD Operations

        /// <summary>
        /// Retrieves all entities of type T from the database
        /// Warning: Loads all records - use specifications for large datasets
        /// </summary>
        /// <returns>Read-only collection of all entities</returns>
        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            // AsNoTracking: Read-only query, better performance
            // No change tracking = less memory, faster queries
            return await _dbContext.Set<T>()
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a single entity by its primary key
        /// </summary>
        /// <param name="id">The primary key value</param>
        /// <returns>The entity if found; otherwise null</returns>
        public async Task<T?> GetByIdAsync(object id)
        {
            // FindAsync is optimized for PK lookups
            // Checks local cache first, then queries database
            return await _dbContext.Set<T>().FindAsync(id);
        }

        /// <summary>
        /// Adds a new entity to the context
        /// Note: Changes are not persisted until Unit of Work commits
        /// </summary>
        /// <param name="entity">The entity to add</param>
        public void Add(T entity)
        {
            _dbContext.Set<T>().Add(entity);
        }

        /// <summary>
        /// Marks an entity as modified
        /// Note: Changes are not persisted until Unit of Work commits
        /// </summary>
        /// <param name="entity">The entity to update</param>
        public void Update(T entity)
        {
            _dbContext.Set<T>().Update(entity);
        }

        /// <summary>
        /// Marks an entity for deletion
        /// Note: Changes are not persisted until Unit of Work commits
        /// </summary>
        /// <param name="entity">The entity to delete</param>
        public void Delete(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
        }

        #endregion

        #region Specification-Based Operations

        /// <summary>
        /// Retrieves entities matching the given specifications
        /// Supports filtering, includes, ordering, and pagination
        /// </summary>
        /// <param name="specifications">Query criteria</param>
        /// <returns>Read-only collection of matching entities</returns>
        public async Task<IReadOnlyList<T>> GetAllWithSpecificationsAsync(
            ISpecifications<T> specifications)
        {

            // Build query using specifications
            // SpecificationsEvaluator applies Where, Include, OrderBy, Skip/Take
            return await ApplySpecifications(specifications).ToListAsync();
        }

        /// <summary>
        /// Retrieves a single entity matching the given specifications
        /// Returns the first match or null
        /// </summary>
        /// <param name="specifications">Query criteria</param>
        /// <returns>First matching entity or null</returns>
        public async Task<T?> GetByIdWithSpecificationsAsync(
            ISpecifications<T> specifications)
        {

            return await ApplySpecifications(specifications).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Counts entities matching the given specifications
        /// Useful for pagination (total record count)
        /// </summary>
        /// <param name="specifications">Query criteria</param>
        /// <returns>Count of matching entities</returns>
        public async Task<int> GetCountWithSpecificationsAsync(
            ISpecifications<T> specifications)
        {

            return await ApplySpecifications(specifications).CountAsync();
        }

        #endregion


        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbContext.Set<T>().Where(predicate).ToListAsync();
        }

        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbContext.Set<T>().CountAsync(predicate);
        }

        private IQueryable<T> ApplySpecifications(ISpecifications<T> spec)
        {
            return SpecificationsEvaluator<T>.CreateQuery(_dbContext.Set<T>(), spec);
        }

    }
}