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
        // Key: Entity type name, Value: Repository instance
        private readonly Hashtable _repositories;

        public UnitOfWork(TalentreeDbContext dbContext)
        {
            _dbContext = dbContext;
            _repositories = new Hashtable();
        }
        public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
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
            // Return the cached repository (cast required due to Hashtable storing objects)
            return (IGenericRepository<TEntity>)_repositories[entityType]!;
        }

        /// <summary>
        /// Commits all pending changes to the database
        /// All repository operations (Add, Update, Delete) are persisted here
        /// </summary>
        /// <returns>Number of entities affected by the operation</returns>
        public async Task<int> CompleteAsync()
            => await _dbContext.SaveChangesAsync();

        // No need To dispose DbContext here as it is managed by the DI container
    }
}
