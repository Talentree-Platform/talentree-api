using System.Collections.Generic;
using System.Threading.Tasks;
using Talentree.Core.Entities;
using Talentree.Core.Specifications;

namespace Talentree.Core.Repository.Contract
{
    /// <summary>
    /// Generic repository interface for data access operations
    /// Provides CRUD operations and specification-based querying
    /// All write operations are tracked but not persisted until Unit of Work commits
    /// </summary>
    /// <typeparam name="T">Entity type that inherits from BaseEntity</typeparam>
    public interface IGenericRepository<T> where T : class
    {
        #region Basic CRUD Operations

        /// <summary>
        /// Retrieves all entities from the database
        /// Warning: Use with caution on large datasets - consider using specifications for filtering
        /// </summary>
        /// <returns>Read-only collection of all entities</returns>
        Task<IReadOnlyList<T>> GetAllAsync();

        /// <summary>
        /// Retrieves a single entity by its primary key
        /// </summary>
        /// <param name="id">The primary key value</param>
        /// <returns>The entity if found; otherwise null</returns>
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Adds a new entity to the context
        /// Note: Changes are not persisted until Unit of Work commits
        /// </summary>
        /// <param name="entity">The entity to add</param>
        void Add(T entity);

        /// <summary>
        /// Marks an entity as modified
        /// Note: Changes are not persisted until Unit of Work commits
        /// </summary>
        /// <param name="entity">The entity to update</param>
        void Update(T entity);

        /// <summary>
        /// Marks an entity for deletion
        /// Note: Changes are not persisted until Unit of Work commits
        /// </summary>
        /// <param name="entity">The entity to delete</param>
        void Delete(T entity);

        #endregion

        #region Specification-Based Operations

        /// <summary>
        /// Retrieves entities that match the given specifications
        /// Supports filtering, includes, ordering, and pagination
        /// </summary>
        /// <param name="specifications">The specification defining query criteria</param>
        /// <returns>Read-only collection of entities matching the specifications</returns>
        Task<IReadOnlyList<T>> GetAllWithSpecificationsAsync(ISpecifications<T> specifications);

        /// <summary>
        /// Retrieves a single entity that matches the given specifications
        /// Returns the first match or null if no entity matches
        /// </summary>
        /// <param name="specifications">The specification defining query criteria</param>
        /// <returns>The first entity matching the specifications, or null if none found</returns>
        Task<T?> GetByIdWithSpecificationsAsync(ISpecifications<T> specifications);

        /// <summary>
        /// Counts entities that match the given specifications
        /// Useful for pagination to determine total page count
        /// </summary>
        /// <param name="specifications">The specification defining query criteria</param>
        /// <returns>The count of matching entities</returns>
        Task<int> GetCountWithSpecificationsAsync(ISpecifications<T> specifications);

        #endregion
    }
}