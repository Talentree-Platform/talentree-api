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
        public ICollection<Expression<Func<T, object>>> Includes { get; } //Includes
    }
}
