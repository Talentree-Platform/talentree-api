using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talentree.Core.Entities;
using Talentree.Core.Specifications;

namespace Talentree.Core.Repository.Contract
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task<IEnumerable<T>> GetAllWithSpecificationsAsync(ISpecifications<T> specifications);
        Task<T?> GetByIdWithSpecificationsAsync(ISpecifications<T> specifications);

        Task<int> GetCountWithSpecificationsAsync(ISpecifications<T> specifications);

    }
}
