using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Talentree.Core.Entities;

namespace Talentree.Core.Specifications
{
    public interface ISpecifications<T> where T : BaseEntity
    {
        public Expression<Func<T, bool>> Criteria { get; } //Where 
        public ICollection<Expression<Func<T, object>>> Includes { get; } //Includes

        public Expression<Func<T, object>> OrderBy { get; } //OrderBy
        public Expression<Func<T, object>> OrderByDescending { get; } //OrderByDescending

    }
}
