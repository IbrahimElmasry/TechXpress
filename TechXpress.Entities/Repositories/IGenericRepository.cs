using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TechXpress.Entities.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        //_context.category.Tolist
        IEnumerable<T> GetAll(Expression<Func<T , bool>> predicate , string? IncludeWord);


        T GetFirstOrDfeault(Expression<Func<T, bool>> predicate, string? IncludeWord);

        void Add(T entity);

        void Remove(T entity);

        void RemoveRange(IEnumerable<T> entities);
    }
}
