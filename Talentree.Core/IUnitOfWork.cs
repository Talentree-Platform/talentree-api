using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talentree.Core.Entities;
using Talentree.Core.Repository.Contract;

namespace Talentree.Core
{
    public interface IUnitOfWork
    {
        /// <summary>
        /// Returns a repository instance for the specified entity type
        /// Multiple calls for the same type return the same cached instance
        /// </summary>
        /// 
        /// <returns>Repository instance for the entity</returns>
        IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;


        /// <summary>
        /// Saves all pending changes to the database as a single transaction
        /// </summary>
        /// <returns>Number of affected rows</returns>
        Task<int> CompleteAsync();
    }
}
