using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talentree.Core;
using Talentree.Core.Entities;
using Talentree.Core.Repository.Contract;
using Talentree.Repository.Data;

namespace Talentree.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly TalentreeDbContext _dbContext;

        // Hashtable stores repository instances to avoid creating duplicates
        // Dictionary instead of Hashtable
        // Key: Entity type name (e.g., "Product")
        // Value: Repository instance (stored as object for generic flexibility)
        private readonly Dictionary<string, object> _repositories;

        public UnitOfWork(TalentreeDbContext dbContext)
        {
            _dbContext = dbContext;
            _repositories = new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets or creates a repository for the specified entity type
        /// Repositories are cached to ensure the same instance is used throughout the Unit of Work lifecycle
        /// </summary>
        public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class
        {
            // Use the entity type name as the key for caching
            var entityType = typeof(TEntity).Name;
            // Check if repository already exists in cache
            if (!_repositories.ContainsKey(entityType))
            {
                // Create new repository instance and cache it
                var repo = new GenericRepository<TEntity>(_dbContext);
                _repositories.Add(entityType, repo);

            }


            // Return the cached repository
            // Cast is still needed because Dictionary stores as object
            // But at least the key is type-safe (string)
            return (IGenericRepository<TEntity>)_repositories[entityType];

        }

        /// <summary>
        /// Commits all pending changes to the database
        /// All repository operations (Add, Update, Delete) are persisted here
        /// </summary>
        /// <returns>Number of entities affected by the operation</returns>
        public async Task<int> CompleteAsync()
            => await _dbContext.SaveChangesAsync();

        /// <summary>
        /// Disposes the database context and clears the repository cache - More Safe
        /// </summary>
        public async ValueTask DisposeAsync()
            => await _dbContext.DisposeAsync();
    }
}
