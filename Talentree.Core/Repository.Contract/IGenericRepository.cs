using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talentree.Core.Entities;

namespace Talentree.Core.Repository.Contract
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
       Task<IReadOnlyList<T>> GetAllAsync();

       Task<T?> GetByIdAsync(int id);
    }
}
